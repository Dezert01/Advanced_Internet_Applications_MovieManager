import os
import json
from django.core.management.base import BaseCommand, CommandError
from django.contrib.auth.hashers import make_password
from django.contrib.auth.models import User
from django.core.management import call_command

class Command(BaseCommand):
    help = 'Loads user data from JSON fixtures and hash their passwords'

    def handle(self, *args, **options):
        # print(os.getcwd())

        fixture_dir = os.path.join('movielens', 'fixtures', 'users')
        

        for filename in os.listdir(fixture_dir):
            if filename.endswith('.json'):
                file_path = os.path.join(fixture_dir, filename)
                with open(file_path, 'r') as file:
                    user_data = json.load(file)
                    hashed = make_password(user_data['password'])
                    user = User.objects.create_user(
                        username=user_data['username'],
                        password=hashed,
                        first_name=user_data['first_name'],
                        last_name=user_data['last_name']
                    )
                    user.save()
                    self.stdout.write(f'Successfully loaded user {user.username}')