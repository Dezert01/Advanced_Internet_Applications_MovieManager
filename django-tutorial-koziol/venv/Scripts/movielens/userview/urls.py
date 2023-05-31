from django.urls import path
from . import views

urlpatterns = [
    # path("", views.index, name="index"),
    # path("genre/<int:genre_id>", views.view_genre, name="index"),
    # path("movie/<int:movie_id>", views.view_movie, name="index"),
    path("", views.index, name="index"),
    path("search", views.SearchView.as_view(), name="search"),
    path("genre/<int:pk>", views.GenreView.as_view(), name="genre"),
    path("movie/<int:pk>", views.MovieView.as_view(), name="movie"),
    path("register", views.register_request, name="register"),
    path("login", views.login_request, name="login"),
    path("logout", views.logout_request, name="logout"),
    path("rated", views.RatedView.as_view(), name="rated"),
    path("search", views.search_movies, name="search_movies"),
    path('rate_movie/', views.rate_movie, name='rate_movie'),
    path('add_comment/', views.add_comment, name='add_comment'),
    path('movie/<int:pk>/edit/', views.MovieEditView.as_view(), name='movie_edit'),
    path('movie_add', views.movie_add, name='movie_add'),
    path('movie/<int:movie_id>/add_image/', views.add_image, name='add_image'),
    path('video', views.video.as_view(), name='video'),
    path("videopage/<int:pk>", views.VideoPage.as_view(), name="videopage"),
]