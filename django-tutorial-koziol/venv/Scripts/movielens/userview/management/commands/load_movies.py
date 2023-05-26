import os
import json
from django.core.management.base import BaseCommand
from userview.models import Movie, Genre

class Command(BaseCommand):
    help = 'Load movies from JSON files'

    def handle(self, *args, **options):
        fixture_dir = os.path.join('movielens', 'fixtures', 'movies')
 
        for filename in os.listdir(fixture_dir):
            if filename.endswith('.json'):
                file_path = os.path.join(fixture_dir, filename)
                with open(file_path, 'r') as file:
                    movie_data = json.load(file)
                    movie = Movie.objects.create(
                        title=movie_data['title'],
                        imdb_link=movie_data['imdbLink'],
                        image=movie_data['image'],
                        year=movie_data['year'],
                        director=movie_data['director'],
                        description=movie_data['description'],
                    )
                    genre_names = movie_data['genre'].split(', ')
                    genres = Genre.objects.filter(name__in=genre_names)
                    movie.genres.set(genres)
                    movie.save()
                    self.stdout.write(f'Successfully loaded movie: {movie.title}')