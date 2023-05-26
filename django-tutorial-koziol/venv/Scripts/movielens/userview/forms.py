from django.contrib.auth.models import User
from django.contrib.auth.forms import UserCreationForm
from django import forms
from .models import Movie, Genre

class NewUserForm(UserCreationForm):
    email = forms.EmailField(required=True)

    class Meta:
        model = User
        fields = ("username", "email", "password1", "password2")

    def save(self, commit=True):
        user = super(NewUserForm, self).save(commit=False)
        user.email = self.cleaned_data['email']

        if commit:
            user.save()
        return user 

class MovieForm(forms.ModelForm):
    class Meta:
        model = Movie
        fields = ['title', 'imdb_link', 'genres']
        widgets = {
            'genres': forms.CheckboxSelectMultiple(attrs={'class': 'form-check-input'}),
        }

# class ImageForm(forms.ModelForm):
#     class Meta:
#         model = Image
#         fields = ['image']