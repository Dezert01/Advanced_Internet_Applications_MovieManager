using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
namespace adv_dotnet.Controllers;
[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase {
    [HttpPost("UploadMovieCsv")]
    public string Post(IFormFile inputFile) {
        MoviesContext dbContext = new MoviesContext();

        var strm = inputFile.OpenReadStream();
        byte[] buffer = new byte[inputFile.Length];
        strm.Read(buffer,0,(int)inputFile.Length);
        string fileContent = System.Text.Encoding.Default.GetString(buffer);
        strm.Close();

        bool skip_header = true;
        foreach(string line in fileContent.Split('\r')) {
            if(skip_header) {
                skip_header =false;
                continue;
            }
            var tokens = line.Split(",");
            if(tokens.Length < 3) continue;
            
            string MovieID = tokens[0];
            // string MovieName = tokens[1];
            string MovieName;
            if (tokens[1].StartsWith("\"") && !tokens[1].EndsWith("\""))
            {
                int i = 2;
                while (!tokens[i].EndsWith("\""))
                {
                    tokens[1] += "," + tokens[i];
                    i++;
                }
                tokens[1] += "," + tokens[i];
                MovieName = tokens[1].Substring(1, tokens[1].Length - 2);
            }
            else
            {
                MovieName = tokens[1];
            }
            
            string[] Genres = tokens[tokens.Length - 1].Split("|");
            List<Genre> movieGenres = new List<Genre>();
            if(Genres.Length > 0 && Genres[0] != "(no genres listed)") {
                foreach(string genre in Genres) {
                    Genre g = new Genre();
                    g.Name = genre;
                    if(!dbContext.Genres.Any(e => e.Name == g.Name)) {
                        dbContext.Genres.Add(g);
                        dbContext.SaveChanges();
                    }
                    IQueryable<Genre> results = dbContext.Genres.Where(e => e.Name == g.Name);
                    if(results.Count() > 0) {
                        movieGenres.Add(results.First());
                    }
                }
            } 
            Movie m = new Movie();
            m.MovieID = int.Parse(MovieID);
            m.Title = MovieName;
            m.Genres = movieGenres;
            if (!dbContext.Movies.Any(e=>e.MovieID == m.MovieID)) dbContext.Movies.Add(m);
            // dbContext.SaveChanges();

        }
        dbContext.SaveChanges();

        return "OK";
    }

    [HttpGet("GetAllGenres")]
    public IEnumerable<Genre> GetAllGenres() {
        MoviesContext dbContext = new MoviesContext();
        return dbContext.Genres.AsEnumerable();
    }

    [HttpGet("GetMoviesByName/{search_phrase}")]
    public IEnumerable<Movie> GetMoviesByName(string search_phrase) {
        MoviesContext dbContext = new MoviesContext();
        return dbContext.Movies.Where(e => e.Title.Contains(search_phrase));
    }

    [HttpPost("GetMoviesByGenre")]
    public IEnumerable<Movie> GetMoviesByGenre(string search_phrase) {
        MoviesContext dbContext = new MoviesContext();
        return dbContext.Movies.Where( m => m.Genres.Any( 
            p => p.Name.Contains(search_phrase)
        ));
    }

    [HttpPost("UploadRatingCsv")]
    public string PostRatings(IFormFile inputFile) {
        MoviesContext dbContext = new MoviesContext();

        var strm = inputFile.OpenReadStream();
        byte[] buffer = new byte[inputFile.Length];
        strm.Read(buffer,0,(int)inputFile.Length);
        string fileContent = System.Text.Encoding.Default.GetString(buffer);
        strm.Close();

        bool skip_header = true;

        List<User> tempUsers = new List<User>();

        foreach(string line in fileContent.Split('\n')) {
            if(skip_header) {
                skip_header =false;
                continue;
            }
            var tokens = line.Split(",");
            if(tokens.Length != 4) continue;
            string UserID = tokens[0];

            if (!tempUsers.Any(u => u.UserID == int.Parse(UserID)) && !dbContext.Users.Any(u => u.UserID == int.Parse(UserID))) {
                User tempUser = new User();
                tempUser.UserID = int.Parse(UserID);
                tempUser.Name = "User" + UserID;
                tempUsers.Add(tempUser);
            }
        }

        dbContext.Users.AddRange(tempUsers);

        dbContext.SaveChanges();

        skip_header = true;
        foreach(string line in fileContent.Split('\n')) {
            if(skip_header) {
                skip_header =false;
                continue;
            }
            var tokens = line.Split(",");
            if(tokens.Length != 4) continue;
            string UserID = tokens[0];
            string MovieID = tokens[1];
            string RatingValue = tokens[2];

            // if (int.Parse(UserID) > 1) {
            //     break;
            // }
            Rating r = new Rating();
            r.RatingValue = RatingValue;
            r.RatingUser = dbContext.Users.Where(e => e.UserID == int.Parse(UserID)).First();
            r.RatedMovie = dbContext.Movies.FirstOrDefault(e => e.MovieID == int.Parse(MovieID));

            if (r.RatedMovie == null) {
                continue;
            }

            dbContext.Ratings.Add(r);
            
            // if(!dbContext.Ratings.Any(e=>e.RatingID == r.RatingID)) dbContext.Ratings.Add(r);
            // dbContext.Ratings.Add(r);

        }
        dbContext.SaveChanges();

        return "OK";
    }

    // T1.1
    [HttpGet("{id}/GetGenres")]
    public IEnumerable<Genre> GetGenres(int id) {
        MoviesContext dbContext = new MoviesContext();
        Movie movie = dbContext.Movies.FirstOrDefault(e => e.MovieID == id);
        if (movie != null) {
            dbContext.Entry(movie)
                .Collection(m => m.Genres)
                .Load();
            List<Genre> genres = new List<Genre>();
            foreach (Genre genre in movie.Genres) {
                genres.Add(genre);
            }
            return genres;
        } else {
            return new List<Genre>();
        }
    }

    // T1.2
    [HttpGet("{id}/GetGenresVector")]
    public int[] GetGenresVector(int id) {
        MoviesContext dbContext = new MoviesContext();
        List<Genre> allGenres = dbContext.Genres.ToList();
        Movie movie = dbContext.Movies.FirstOrDefault(e => e.MovieID == id);
        if (movie != null) {
            dbContext.Entry(movie)
                .Collection(m => m.Genres)
                .Load();
            int[] genres = new int[allGenres.Count];
            foreach (Genre genre in movie.Genres) {
                int genreIndex = allGenres.FindIndex(g => g.GenreID == genre.GenreID);
                genres[genreIndex] = 1;
            }
            return genres;
        } else {
            return new int[allGenres.Count];
        }
    }

    private double CalculateCosineSimilarity(int[] vec1, int[] vec2) {
        double dotProduct = 0;
        double mag1 = 0;
        double mag2 = 0;
        for (int i = 0; i < vec1.Length; i++) {
            dotProduct += vec1[i] * vec2[i];
            mag1 += vec1[i] * vec1[i];
            mag2 += vec2[i] * vec2[i];
        }
        mag1 = Math.Sqrt(mag1);
        mag2 = Math.Sqrt(mag2);
        return dotProduct / (mag1 * mag2);
    }

    private double CalculateCosineSimilarityDouble(double[] vec1, double[] vec2) {
        double dotProduct = 0;
        double mag1 = 0;
        double mag2 = 0;
        for (int i = 0; i < vec1.Length; i++) {
            dotProduct += vec1[i] * vec2[i];
            mag1 += vec1[i] * vec1[i];
            mag2 += vec2[i] * vec2[i];
        }
        mag1 = Math.Sqrt(mag1);
        mag2 = Math.Sqrt(mag2);
        return dotProduct / (mag1 * mag2);
    }


    // T1.3
    [HttpGet("GetMoviesSimilarity/{id1}/{id2}")]
    public double GetMoviesSimilarity(int id1, int id2) {
        MoviesContext dbContext = new MoviesContext();
        Movie movie1 = dbContext.Movies.FirstOrDefault(e => e.MovieID == id1);
        Movie movie2 = dbContext.Movies.FirstOrDefault(e => e.MovieID == id2);
        if (movie1 != null && movie2 != null) {
            dbContext.Entry(movie1)
                .Collection(m => m.Genres)
                .Load();
            dbContext.Entry(movie2)
                .Collection(m => m.Genres)
                .Load();

            List<int> allGenres = dbContext.Genres.Select(g => g.GenreID).ToList();

            int[] m1Genres = new int[allGenres.Count];
            int[] m2Genres = new int[allGenres.Count];

            foreach (Genre genre in movie1.Genres) {
                int index = allGenres.IndexOf(genre.GenreID);
                if (index >= 0) {
                    m1Genres[index] = 1;
                }
            }

            foreach (Genre genre in movie2.Genres) {
                int index = allGenres.IndexOf(genre.GenreID);
                if (index >= 0) {
                    m2Genres[index] = 1;
                }
            }

            return CalculateCosineSimilarity(m1Genres, m2Genres);
        } else {
            return 0;
        }
    }

    //T1.4
    [HttpGet("{id}/GetSimilarMovies")]
    public List<Movie> GetSimilarMovies(int id) {
        MoviesContext dbContext = new MoviesContext();

        Movie movie = dbContext.Movies.FirstOrDefault(e => e.MovieID == id);
        if (movie == null) {
            return new List<Movie>();
        }

        dbContext.Entry(movie).Collection(m => m.Genres).Load();
        List<int> genreIds = movie.Genres.Select(g => g.GenreID).ToList();

        List<Movie> similarMovies = dbContext.Movies
            .Where(m => m.MovieID != id && m.Genres.Any(g => genreIds.Contains(g.GenreID)))
            .ToList();

        return similarMovies;
    }

    //T1.5
    [HttpGet("{id}/GetSimilarMoviesCosine/{threshold}")]
    public List<Movie> GetSimilarMoviesCosine(int id, double threshold) {
        MoviesContext dbContext = new MoviesContext();
        Movie movie = dbContext.Movies.FirstOrDefault(e => e.MovieID == id);

        if (movie == null) {
            return new List<Movie>();
        }
    
        List<int> allGenres = dbContext.Genres.Select(g => g.GenreID).ToList();
        int[] movieGenres = new int[allGenres.Count];

        dbContext.Entry(movie)
            .Collection(m => m.Genres)
            .Load();

        foreach (Genre genre in movie.Genres) {
            int index = allGenres.IndexOf(genre.GenreID);
            if (index >= 0) {
                movieGenres[index] = 1;
            }
        }

        IEnumerable<Movie> allMovies = dbContext.Movies.ToList();
        List<Movie> similarMovies = new List<Movie>();
        foreach (Movie otherMovie in allMovies) {
            if (otherMovie.MovieID == id) {
                continue;
            }
            if (otherMovie == null) {
                continue;
            }
            dbContext.Entry(otherMovie)
                .Collection(m => m.Genres)
                .Load();
            int[] otherGenres = new int[allGenres.Count];
            foreach (Genre otherGenre in otherMovie.Genres) {
                int index = allGenres.IndexOf(otherGenre.GenreID);
                if (index >= 0) {
                    otherGenres[index] = 1;
                }
            }
            double similarity = CalculateCosineSimilarity(movieGenres, otherGenres);
            if (similarity >= threshold) {
                similarMovies.Add(otherMovie);
            }
        }
        return similarMovies;
    }      

    //T1.6
    [HttpGet("GetMoviesRatedByUser/{id}")]
    public List<Movie> GetMoviesRatedByUser(int id) {
        MoviesContext dbContext = new MoviesContext();
        List<Movie> ratedMovies = dbContext.Ratings
            .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
            .Select(r => r.RatedMovie)
            .Distinct()
            .ToList();
        return ratedMovies;
    }

    //T1.7
    [HttpGet("GetSortedMoviesRatedByUser/{id}")]
    public List<Movie> GetSortedMoviesRatedByUser(int id) {
        MoviesContext dbContext = new MoviesContext();
        List<Movie> ratedMovies = dbContext.Ratings
            .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
            .OrderByDescending(r => r.RatingValue)
            .Select(r => r.RatedMovie)
            .ToList();
        return ratedMovies;
    }

    //T1.8
    [HttpGet("GetSimilarToHighestRated/{id}/{threshold}")]
    public List<Movie> GetSimilarToHighestRated(int id, double threshold) {
        MoviesContext dbContext = new MoviesContext();
        List<Movie> ratedMovies = dbContext.Ratings
            .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
            .OrderByDescending(r => r.RatingValue)
            .Select(r => r.RatedMovie)
            .ToList();

        Movie movie = ratedMovies.FirstOrDefault();

        // Movie movie = dbContext.Movies.FirstOrDefault(e => e.MovieID == highestRated.MovieID);
        if (movie == null) {
            return new List<Movie>();
        }

        List<int> allGenres = dbContext.Genres.Select(g => g.GenreID).ToList();
        int[] movieGenres = new int[allGenres.Count];

        dbContext.Entry(movie)
            .Collection(m => m.Genres)
            .Load();

        foreach (Genre genre in movie.Genres) {
            int index = allGenres.IndexOf(genre.GenreID);
            if (index >= 0) {
                movieGenres[index] = 1;
            }
        }

        IEnumerable<Movie> allMovies = dbContext.Movies.ToList();
        List<Movie> similarMovies = new List<Movie>();
        foreach (Movie otherMovie in allMovies) {
            if (otherMovie.MovieID == id) {
                continue;
            }
            if (otherMovie == null) {
                continue;
            }
            dbContext.Entry(otherMovie)
                .Collection(m => m.Genres)
                .Load();
            int[] otherGenres = new int[allGenres.Count];
            foreach (Genre otherGenre in otherMovie.Genres) {
                int index = allGenres.IndexOf(otherGenre.GenreID);
                if (index >= 0) {
                    otherGenres[index] = 1;
                }
            }
            double similarity = CalculateCosineSimilarity(movieGenres, otherGenres);
            if (similarity >= threshold) {
                similarMovies.Add(otherMovie);
            }
        }
        return similarMovies;
    }

    //T1.9
    [HttpGet("GetRecommendationList/{id}/{threshold}/{size}")]
    public List<Movie> GetRecommendationList(int id, double threshold, int size) {
        MoviesContext dbContext = new MoviesContext();

        Movie highestRatedMovie = dbContext.Ratings
            .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
            .OrderByDescending(r => r.RatingValue)
            .Select(r => r.RatedMovie)
            .FirstOrDefault();

        if (highestRatedMovie == null) {
            return new List<Movie>();
        }

        List<int> allGenres = dbContext.Genres.Select(g => g.GenreID).ToList();
        int[] movieGenres = new int[allGenres.Count];

        dbContext.Entry(highestRatedMovie)
            .Collection(m => m.Genres)
            .Load();

        foreach (Genre genre in highestRatedMovie.Genres) {
            int index = allGenres.IndexOf(genre.GenreID);
            if (index >= 0) {
                movieGenres[index] = 1;
            }
        }

        IEnumerable<Movie> notRatedMovies = dbContext.Movies
            .Where(m => !dbContext.Ratings.Any(r => r.RatingUser.UserID == id && r.RatedMovie.MovieID == m.MovieID))
            .ToList();

        List<Movie> recommendationList = new List<Movie>();
        foreach (Movie movie in notRatedMovies) {
            if (recommendationList.Count >= size) {
                break;
            }
            if (movie.MovieID == highestRatedMovie.MovieID) {
                continue;
            }
            if (movie == null) {
                continue;
            }
            dbContext.Entry(movie)
                .Collection(m => m.Genres)
                .Load();
            int[] otherGenres = new int[allGenres.Count];
            foreach (Genre otherGenre in movie.Genres) {
                int index = allGenres.IndexOf(otherGenre.GenreID);
                if (index >= 0) {
                    otherGenres[index] = 1;
                }
            }
            double similarity = CalculateCosineSimilarity(movieGenres, otherGenres);
            if (similarity >= threshold) {
                recommendationList.Add(movie);
            }
        }

        return recommendationList;
    }


    // T2
    // [HttpGet("GetSortedRatingsOfMoviesRatedByUser/{id}")]
    // public List<string> GetSortedRatingsOfMoviesRatedByUser(int id) {
    //     MoviesContext dbContext = new MoviesContext();
    //     List<string> ratingValues = dbContext.Ratings
    //         .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
    //         .OrderByDescending(r => r.RatingValue)
    //         .Select(r => r.RatingValue)
    //         .ToList();
    //     return ratingValues;
    // }

    // [HttpGet("GetSortedMoviesRatedByUserddsadas/{id}")]
    // public List<int> GetSortedMoviesRatedByUserddsadas(int id) {
    //     MoviesContext dbContext = new MoviesContext();
    //     List<int> ratedMovieIds = dbContext.Ratings
    //         .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
    //         .OrderByDescending(r => r.RatingValue)
    //         .Select(r => r.RatedMovie.MovieID)
    //         .ToList();
    //     return ratedMovieIds;
    // }

    [HttpGet("GetRecommendationH2/user/{id}/{threshold}")]
    public List<Movie> GetRecommendationH2(int id, double threshold) {
        MoviesContext dbContext = new MoviesContext();


        // Get all rating values of movies rated by given user
        List<string> ratingValuesString = dbContext.Ratings
            .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
            .OrderByDescending(r => r.RatingValue)
            .Select(r => r.RatingValue)
            .ToList();

        // Convert string to Double
        List<double> ratingValues = ratingValuesString.Select(s => Double.Parse(s, CultureInfo.InvariantCulture)).ToList();

        // Get move Ids of above ratings
        List<int> ratedMovieIds = dbContext.Ratings
            .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
            .OrderByDescending(r => r.RatingValue)
            .Select(r => r.RatedMovie.MovieID)
            .ToList();

        IEnumerable<User> allUsers = dbContext.Users.ToList();
        List<User> similarUsers = new List<User>();
        foreach (User otherUser in allUsers) {
            if (otherUser.UserID == id) {
                continue;
            }
            if (otherUser == null) {
                continue;
            }

            List<double> otherRatingValues = new List<double>();
            var ratings = dbContext.Ratings.Where(r => r.RatingUser.UserID == otherUser.UserID && r.RatedMovie != null);

            // We loop through movies of our given user and check if other user has rated the movie or not. The result of that loop is vector of rating values
            foreach(var movieId in ratedMovieIds) {
                var rating = dbContext.Ratings.FirstOrDefault(r => r.RatingUser.UserID == otherUser.UserID && r.RatedMovie.MovieID == movieId);
                double ratingValue = rating != null ? Double.Parse(rating.RatingValue, CultureInfo.InvariantCulture) : 0.0;
                otherRatingValues.Add(ratingValue);
            }

            // get the Cosine Similarity
            double similarity = CalculateCosineSimilarityDouble(ratingValues.ToArray(), otherRatingValues.ToArray());

            if (similarity >= threshold) {
                similarUsers.Add(otherUser);
            }
        }


        
        List<Movie> recommendationList = new List<Movie>();
        // Loop through Similar Users we got above and get the highest rated movie from each User. If our given User has rated the movie we don't add it.
        foreach (User user in similarUsers) {
            List <Movie> ratedMovies = dbContext.Ratings
                .Where(r => r.RatingUser.UserID == user.UserID && r.RatedMovie != null)
                .OrderByDescending(r => r.RatingValue)
                .Select(r => r.RatedMovie)
                .ToList();

            foreach (Movie movie in ratedMovies) {
                if (!ratedMovieIds.Contains(movie.MovieID) && !recommendationList.Contains(movie)) {
                    recommendationList.Add(movie);
                    break;
                }
            }
        }

        return recommendationList;
    }

    //T3 We get a list From T2. Then we get cosine similarity from T1 comarping highest rated movie of given user and each movie from T2. We got different threshold for user similarity and movie similarity. We also have a size of recommendation List
    [HttpGet("GetRecommendationH3/user/{id}/{thresholdUser}/{thresholdMovie}/{size}")]
    public List<Movie> GetRecommendationH2(int id, double thresholdUser, double thresholdMovie, int size) {
        MoviesContext dbContext = new MoviesContext();

        List<Movie> movies = GetRecommendationH2(id, thresholdUser);

        Movie highestRatedMovie = dbContext.Ratings
            .Where(r => r.RatingUser.UserID == id && r.RatedMovie != null)
            .OrderByDescending(r => r.RatingValue)
            .Select(r => r.RatedMovie)
            .FirstOrDefault();

        if (highestRatedMovie == null) {
            return new List<Movie>();
        }

        List<int> allGenres = dbContext.Genres.Select(g => g.GenreID).ToList();
        int[] movieGenres = new int[allGenres.Count];

        dbContext.Entry(highestRatedMovie)
            .Collection(m => m.Genres)
            .Load();

        foreach (Genre genre in highestRatedMovie.Genres) {
            int index = allGenres.IndexOf(genre.GenreID);
            if (index >= 0) {
                movieGenres[index] = 1;
            }
        }

        List<Movie> recommendationList = new List<Movie>();
        foreach (Movie movie in movies) {
            if (recommendationList.Count >= size) {
                break;
            }
            if (movie.MovieID == highestRatedMovie.MovieID) {
                continue;
            }
            if (movie == null) {
                continue;
            }
            // We do that to include genres as we don't have them in GetRecommendationH2.
            // The other way to achieve is that we create new list of Movies of ids that are in "List<Movie> movies = GetRecommendationH2(id, thresholdUser);" But this way is faster :)
            dbContext.Entry(movie)
                .State = EntityState.Modified;
            dbContext.Entry(movie)
                .Collection(m => m.Genres)
                .Load();
            int[] otherGenres = new int[allGenres.Count];
            foreach (Genre otherGenre in movie.Genres) {
                int index = allGenres.IndexOf(otherGenre.GenreID);
                if (index >= 0) {
                    otherGenres[index] = 1;
                }
            }
            double similarity = CalculateCosineSimilarity(movieGenres, otherGenres);
            if (similarity >= thresholdMovie) {
                recommendationList.Add(movie);
            }
        }

        return recommendationList;
    }
}
