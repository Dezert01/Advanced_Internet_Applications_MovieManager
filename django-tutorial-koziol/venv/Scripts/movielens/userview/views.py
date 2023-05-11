# from django.shortcuts import render, get_object_or_404
# from django.http import HttpRequest, HttpResponse
# from django.template import loader
# from .models import Movie, Genre

# def index(request : HttpRequest):
#     movies = Movie.objects.order_by('-title')
#     template = loader.get_template('userview/index.html')
#     context = {
#         'movies' : movies
#     }
#     return HttpResponse(template.render(context,request))

# def view_movie(request: HttpRequest, movie_id):

#     template = loader.get_template('userview/movie.html')
#     movie = get_object_or_404(Movie, id=movie_id)
#     context = {
#         'title': movie.title
#     }
#     return HttpResponse(template.render(context,request))

# def view_genre(request: HttpRequest, genre_id):

#     template = loader.get_template('userview/genre.html')
#     genre = get_object_or_404(Genre, id=genre_id)

#     context = {
#         'name': genre.name
#     }

#     return HttpResponse(template.render(context,request))


from django.views import generic
from .models import Movie, Genre, Rating
from django.contrib.auth import login, logout, authenticate
from .forms import NewUserForm
from django.shortcuts import render
from django.contrib import messages
from django.shortcuts import redirect, get_object_or_404
from django.contrib.auth.forms import AuthenticationForm


class IndexView(generic.ListView):
    template_name = 'userview/index.html'
    context_object_name = 'movies'
    paginate_by = 10

    def get_queryset(self):
        user = self.request.user
        rated_movies = Rating.objects.filter(user=user).select_related('movie')
        return [rating.movie for rating in rated_movies]
class MovieView(generic.DetailView):
    model = Movie
    template_name = 'userview/movie.html'
class GenreView(generic.DetailView):
    model = Genre
    template_name = 'userview/genre.html'


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


def movie_rating(request):
    # print(request.movie_id)
    if request.user.is_authenticated:
        print('1')
        if request.method == "POST":
            print(2)
            ratings = Rating.objects.filter(user=request.user)
            movies = [rating.movie for rating in ratings]

            movie_id = request.POST["movie_id"]
            rating_value = int(request.POST['rating_value'])
            movie = get_object_or_404(Movie, id=movie_id)

            # Create or update the rating object
            rating, created = Rating.objects.update_or_create(
                user=request.user,
                movie=movie,
                defaults={'value': rating_value}
            )

            # Set a message based on whether the rating was added or updated
            if created:
                message = 'Rating added successfully.'
            else:
                message = 'Rating updated successfully.'

            return redirect("index")
        else:
            print(3)
            message = ''

            # Render the template with the list of movies and a message (if any)
            context = {
                'movies': movies,
                'message': message,
            }
            return redirect("index")
    else:
        print('asd')
        return redirect("index")