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
from .models import Movie, Genre

class IndexView(generic.ListView):
    paginate_by = 1
    template_name = 'userview/index.html'
    context_object_name = 'movies'
    def get_queryset(self):
        return Movie.objects.order_by('-title')
class MovieView(generic.DetailView):
    model = Movie
    template_name = 'userview/movie.html'
class GenreView(generic.DetailView):
    model = Genre
    template_name = 'userview/genre.html'