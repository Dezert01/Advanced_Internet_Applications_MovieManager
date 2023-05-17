from django.urls import path
from . import views

urlpatterns = [
    # path("", views.index, name="index"),
    # path("genre/<int:genre_id>", views.view_genre, name="index"),
    # path("movie/<int:movie_id>", views.view_movie, name="index"),
    path("", views.IndexView.as_view(), name="index"),
    path("genre/<int:pk>", views.GenreView.as_view(), name="genre"),
    path("movie/<int:pk>", views.MovieView.as_view(), name="movie"),
    path("register", views.register_request, name="register"),
    path("login", views.login_request, name="login"),
    path("logout", views.logout_request, name="logout"),
    path("rated", views.RatedView.as_view(), name="rated"),
    path("search", views.search_movies, name="search_movies"),
]