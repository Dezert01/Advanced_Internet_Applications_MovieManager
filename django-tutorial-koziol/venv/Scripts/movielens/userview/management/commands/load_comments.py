import os
import json
from django.core.management.base import BaseCommand
from userview.models import Rating, Movie, Comment
from django.contrib.auth.models import User
from django.utils import timezone

class Command(BaseCommand):
    help = 'Load movies from JSON files'

    def handle(self, *args, **options):
        file_path = os.path.join('movielens', 'fixtures', 'comments.json')

        with open(file_path, 'r') as file:
            comments_data = json.load(file)
            for comment_data in comments_data:
                try:
                    movie = Movie.objects.get(id=comment_data['movie']) 
                    user = User.objects.get(id=comment_data['user'])
                    timestamp = timezone.datetime.fromtimestamp(int(comment_data['timestamp']))
                    comment = Comment.objects.create(
                        content=comment_data['comment'],
                        movie=movie,
                        user=user,
                        timestamp=timestamp
                    )
                    comment.save()
                    self.stdout.write(f'Successfully loaded comment')
                except Movie.DoesNotExist:
                    print('No Movie')
                except User.DoesNotExist:
                    print('No User')