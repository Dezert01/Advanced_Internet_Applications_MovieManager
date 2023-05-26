import os
import json
from django.core.management.base import BaseCommand
from userview.models import Rating, Movie
from django.contrib.auth.models import User

class Command(BaseCommand):
    help = 'Load movies from JSON files'

    def handle(self, *args, **options):
        fixture_dir = os.path.join('movielens', 'fixtures', 'ratings')
        
        for filename in os.listdir(fixture_dir):
            if filename.endswith('.json'):
                file_path = os.path.join(fixture_dir, filename)
                with open(file_path, 'r') as file:
                    rating_data = json.load(file)
                    try:
                        movie = Movie.objects.get(id=rating_data['movie_id']) 
                        user = User.objects.get(id=rating_data['user_id'])
                        val = int(float(rating_data['rating']))
                        rating = Rating.objects.create(
                            value=val,
                            movie=Movie.objects.get(id=rating_data['movie_id']),
                            user=User.objects.get(id=rating_data['user_id'])
                        )
                        rating.save()
                        self.stdout.write(f'Successfully loaded rating')
                    except Movie.DoesNotExist:
                        print('No Movie')
                    except User.DoesNotExist:
                        print('No User')