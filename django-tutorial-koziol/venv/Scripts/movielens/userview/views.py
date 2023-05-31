from django.views import generic
from .models import Movie, Genre, Rating, Comment, EmbeddedVideoItem
from django.contrib.auth import login, logout, authenticate
from .forms import NewUserForm, MovieForm, ImageForm
from django.shortcuts import render
from django.db.models import Avg, Max
from django.contrib import messages
from django.shortcuts import redirect, get_object_or_404
from django.contrib.auth.forms import AuthenticationForm
from django.urls import reverse_lazy
from random import choice

def index(request):
    
    recently_rated_movies = Movie.objects.annotate(
        latest_rating=Max('rating__timestamp')
    ).order_by('-latest_rating')[:20]

    recently_rated_movie_ids = [movie.id for movie in recently_rated_movies]

    top_movies = Movie.objects.filter(id__in=recently_rated_movie_ids).annotate(
        avg_rating=Avg('rating__value')
    ).order_by('-avg_rating')[:3]
    
    similar_movies = []
    if request.user.is_authenticated:
        rated_movies = Rating.objects.filter(user=request.user, value__gte=4.0).values_list('movie', flat=True)
        if rated_movies:
            random_movie_id = choice(rated_movies)
            random_movie = Movie.objects.get(id=random_movie_id)
            similar_movies = random_movie.get_similar_movies()

    context = {
        'top_movies': top_movies,
        'similar_movies': similar_movies
    }
    return render(request, 'userview/index.html', context)

class SearchView(generic.ListView):
    template_name = 'userview/search.html'
    context_object_name = 'movies'
    paginate_by = 10
    queryset = Movie.objects.order_by('-title')

    def get_queryset(self):
        queryset = super().get_queryset()

        genre = self.request.GET.get('genre')
        title = self.request.GET.get('title')
        min_rating = self.request.GET.get('min_rating')

        # Apply search filters based on the search parameters
        if genre:
            queryset = queryset.filter(genres__name=genre)
        if title:
            queryset = queryset.filter(title__icontains=title)
        if min_rating:
            queryset = queryset.filter(rating__value__gte=min_rating)

        return queryset.order_by('-title')
        # return Movie.objects.order_by('-title')
    
class MovieView(generic.DetailView):
    model = Movie
    template_name = 'userview/movie.html'
    context_object_name = 'movie'

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context['user'] = self.request.user

        if self.request.user.is_authenticated:
            context['user_rating'] = self.object.rating_set.filter(user=self.request.user).first()
        else:
            context['user_rating'] = None

        context['image_form'] = ImageForm()

        return context

class GenreView(generic.DetailView):
    model = Genre
    template_name = 'userview/genre.html'

class RatedView(generic.ListView):
    template_name = 'userview/rated.html'
    context_object_name = 'movies'
    paginate_by = 10

    # def dispatch(self, request):
    #     if self.request.user.is_anonymous:
    #         # print("AAAAAAAAAAAAAAAAAAAAAAAA")
    #         return redirect('login')
    #     return super().dispatch(request)

    def get_queryset(self):
        user = self.request.user
        if self.request.user.is_anonymous:
            return []
        print(user)
        rated_movies = Rating.objects.filter(user=user).select_related('movie')
        return [rating.movie for rating in rated_movies]
        # return Movie.objects.order_by('-title')


def register_request(request):
    form = NewUserForm()

    if request.method == "POST":
        form = NewUserForm(request.POST)
        if form.is_valid():
            user = form.save()
            login(request, user)
            messages.success(request, "Registration successful.")
            return redirect("index")
        messages.error(request, "Unsuccessful registration. Invalid information.")

    return render (request=request,  template_name="userview/register.html", context={"register_form":form})

def login_request(request):

    if request.method == "POST":
        # form = NewUserForm(request.POST)
        form = AuthenticationForm(request, data=request.POST)
        print("test")
        if form.is_valid():
            print('123')
            # user = form.save()
            username = form.cleaned_data.get('username')
            password = form.cleaned_data.get('password')

            user = authenticate(username=username, password=password)

            if user is not None:
                login(request, user)

            messages.success(request, "Login successful.")
            return redirect("index")
        messages.error(request, "Unsuccessful login. Invalid information.")
    
    form = AuthenticationForm()
    return render (request=request,  template_name="userview/login.html", context={"login_form":form})

def logout_request(request):
    logout(request)
    return redirect("login")
    

def search_movies(request):
    if request.method == 'GET':
        genre = request.GET.get('genre')
        title = request.GET.get('title')
        min_rating = request.GET.get('min_rating')

        # Construct the base queryset for movies
        movies = Movie.objects.all()

        # Apply filters based on search parameters
        if genre:
            movies = movies.filter(genres__name=genre)
        if title:
            movies = movies.filter(title__icontains=title)
        if min_rating:
            movies = movies.filter(rating__value__gte=min_rating)

        context = {'movies': movies}
        return render(request, 'index.html', context)

def rate_movie(request):
    if request.method == 'POST':
        movie_id = request.POST.get('movie_id')
        rating_value = request.POST.get('rating_value')
        movie = get_object_or_404(Movie, pk=movie_id)
        user = request.user

        # Check if the user has already rated the movie
        rating = Rating.objects.filter(movie=movie, user=user).first()

        if rating:
            # If the user has already rated the movie, update the rating value
            rating.value = rating_value
            rating.save()
        else:
            # If the user has not rated the movie, create a new rating instance
            rating = Rating.objects.create(movie=movie, user=user, value=rating_value)

        return redirect('movie', pk=movie_id)

    return redirect('movie', pk=movie_id)

def add_comment(request):
    if request.method == 'POST':
        movie_id = request.POST.get('movie_id')
        content = request.POST.get('content')
        movie = get_object_or_404(Movie, pk=movie_id)
        Comment.objects.create(content=content, movie=movie, user=request.user)
        return redirect('movie', pk=movie_id)

    return redirect('movie', pk=movie_id)

class MovieEditView(generic.UpdateView):
    model = Movie
    template_name = 'userview/movie_edit.html'
    fields = ['title', 'genres']

    def test_func(self):
        return self.request.user.is_superuser
    
    def get_success_url(self):
        return reverse_lazy('movie', kwargs={'pk': self.object.pk})
    
def movie_add(request):
    if request.method == 'POST':
        form = MovieForm(request.POST)
        if form.is_valid():
            movie = form.save()
            return redirect('movie', pk=movie.pk)
    else:
        form = MovieForm()
        genres = Genre.objects.all()
    
    return render(request, 'userview/movie_add.html', {'form': form, 'genres': genres})

def add_image(request, movie_id):
    movie = get_object_or_404(Movie, pk=movie_id)
    
    if request.method == 'POST':
        form = ImageForm(request.POST, request.FILES, instance=movie)
        if form.is_valid():
            form.save()
            return redirect('movie', pk=movie_id)
    else:
        form = ImageForm(instance=movie)
    
    return render(request, 'add_image.html', {'form': form, 'movie': movie})

class video(generic.ListView):
    template_name = 'userview/video.html'
    context_object_name = 'videos'
    paginate_by = 1
    queryset = EmbeddedVideoItem.objects.order_by('-title')

    def get_queryset(self):
        queryset = super().get_queryset()

        return queryset.order_by('-title')

class VideoPage(generic.DetailView):
    model = EmbeddedVideoItem
    template_name = 'userview/videopage.html'
    context_object_name = 'video'
    
