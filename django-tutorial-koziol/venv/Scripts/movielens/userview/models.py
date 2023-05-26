from django.db import models
from django.conf import settings
from django.db.models import Avg
from django.utils.timezone import now

class Genre(models.Model):
    name = models.CharField(max_length=300)
    def __str__(self):
        return self.name
    
class Movie(models.Model):
    title = models.CharField(max_length=1000)
    genres = models.ManyToManyField(Genre)
    director = models.CharField(max_length=1000, blank=True, null=True)
    year = models.CharField(max_length=4, blank=True, null=True)
    imdb_link = models.URLField(blank=True, null=True)
    image = models.ImageField(upload_to='movie_images/', blank=True, null=True)
    description = models.TextField(blank=True, null=True)

    def __str__(self):
        return self.title
    
    def average_rating(self):
        return self.rating_set.aggregate(Avg('value'))['value__avg']
    
    def get_similar_movies(self):
        similar_movies = []

        movie_genres = self.genres.all()

        for movie in Movie.objects.exclude(pk=self.pk):
            other_genres = movie.genres.all()
            shared_genres = movie_genres.intersection(other_genres)

            if len(shared_genres) >= len(movie_genres) / 2:
                similar_movies.append(movie)

        return similar_movies

class Rating(models.Model):
    value = models.IntegerField()
    movie = models.ForeignKey(Movie, on_delete=models.CASCADE)
    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE)
    def __str__(self):
        return f"{self.movie.title}, {self.user.username} ({self.value})"
    
class Comment(models.Model):
    content = models.TextField(max_length = 1000)
    movie = models.ForeignKey(Movie, on_delete = models.CASCADE)
    user = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE)
    timestamp = models.DateTimeField(default=now, editable=False)

    def __str__(self):
        return f"Comment by {self.user.username} on {self.movie.title}"


# class Image(models.Model):
#     movie = models.ForeignKey(Movie, on_delete=models.CASCADE, related_name='images')
#     image = models.ImageField(upload_to='movie_images/')

#     def __str__(self):
#         return f"Image {self.image.name} for {self.movie.title}"