using Microsoft.AspNetCore.Mvc;
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
    [HttpPost("{id}/GetGenres")]
    public List<string> GetGenres(int id) {
        MoviesContext dbContext = new MoviesContext();
        Movie movie = dbContext.Movies.FirstOrDefault(e => e.MovieID == id);
        if (movie != null) {
             dbContext.Entry(movie)
                .Collection(m => m.Genres)
                .Load();
            List<string> genres = new List<string>();
            foreach (Genre genre in movie.Genres) {
                genres.Add(genre.Name);
            }
            return genres;
        } else {
            return new List<string>();
        }
    }

    // T1.2
    [HttpPost("{id}/GetGenresVector")]
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

    // T1.3
    [HttpPost("GetMoviesSimilarity/{id1}/{id2}")]
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

            double dotProduct = 0;
            double normM1 = 0;
            double normM2 = 0;

            for (int i = 0; i < allGenres.Count; i++) {
                dotProduct += m1Genres[i] * m2Genres[i];
                normM1 += m1Genres[i] * m1Genres[i];
                normM2 += m2Genres[i] * m2Genres[i];
            }

            double similarity = dotProduct / (Math.Sqrt(normM1) * Math.Sqrt(normM2));

            return similarity;
        } else {
            return 0;
        }
    }
}
