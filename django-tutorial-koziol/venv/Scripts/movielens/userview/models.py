from django.db import models
from django.conf import settings
from django.db.models import Avg

class Genre(models.Model):
    name = models.CharField(max_length=300)
    def __str__(self):
        return self.name
    
class Movie(models.Model):
    title = models.CharField(max_length=1000)
    genres = models.ManyToManyField(Genre)

    def __str__(self):
        return self.title
    
    def average_rating(self):
        return self.rating_set.aggregate(Avg('value'))['value__avg']

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

class Image(models.Model):
    movie = models.ForeignKey(Movie, on_delete=models.CASCADE, related_name='images')
    image = models.ImageField(upload_to='movie_images/')

    def __str__(self):
        return f"Image for {self.movie.title}"