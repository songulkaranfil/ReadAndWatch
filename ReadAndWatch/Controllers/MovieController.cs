using Microsoft.AspNetCore.Mvc;
using ReadAndWatch.Data;
using ReadAndWatch.Models;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore; // Include için

namespace ReadAndWatch.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;


        public MovieController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }
       


        public IActionResult Index(string genre, string language, int? runtimeMin, int? runtimeMax, int? releaseYear)
        {
            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrEmpty(genre))
                query = query.Where(m => m.Genres != null && m.Genres.Contains(genre));

            if (!string.IsNullOrEmpty(language))
                query = query.Where(m => m.OriginalLanguage != null && m.OriginalLanguage.ToLower() == language.ToLower());

            if (runtimeMin.HasValue)
                query = query.Where(m => m.Runtime >= runtimeMin);
            if (runtimeMax.HasValue)
                query = query.Where(m => m.Runtime <= runtimeMax);

            if (releaseYear.HasValue)
                query = query.Where(m => m.ReleaseDate.HasValue && m.ReleaseDate.Value.Year == releaseYear);

            var filteredMovies = query.OrderByDescending(m => m.VoteAverage).ToList();
            return View(filteredMovies);
        }

        [HttpPost]
        public IActionResult AddFavoriteMovie(int movieId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                // Giriş yapılmamış → login sayfasına yönlendir
                return RedirectToAction("Login", "User");
            }

            if (int.TryParse(userIdClaim.Value, out int userId))
            {
                var exists = _context.FavoriteMovies
                    .FirstOrDefault(f => f.UserId == userId && f.MovieId == movieId);

                if (exists == null)
                {
                    _context.FavoriteMovies.Add(new FavoriteMovie
                    {
                        UserId = userId,
                        MovieId = movieId
                    });
                    _context.SaveChanges();
                }

                // Başarıyla eklenince favoriler sayfasına yönlendir
                return RedirectToAction("Favorites" , "User");
            }

            return Unauthorized();
        }

        public IActionResult Details(int id)
        {
            var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
            if (movie == null)
                return NotFound();

            return View(movie);
        }

        [Authorize]
        public IActionResult Favorites()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var favoriteMovies = _context.FavoriteMovies
                .Where(f => f.UserId == userId)
                .Include(f => f.Movie)
                .Select(f => f.Movie)
                .ToList();

            return View(favoriteMovies);
        }


       



        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Recommend()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            // Kullanıcının favori film ID'leri
            var favoriteIds = _context.FavoriteMovies
                .Where(f => f.UserId == userId)
                .Select(f => f.MovieId)
                .ToList();

            if (!favoriteIds.Any())
                return View("RecommendMovies", new List<Movie>());

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://127.0.0.1:8001/recommend_movies", new { favorite_ids = favoriteIds });

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("🚨 Flask API başarısız yanıt döndürdü.");
                return View("RecommendMovies", new List<Movie>());
            }

            var result = await response.Content.ReadFromJsonAsync<RecommendationResponse>();

            if (result?.Recommendations == null || !result.Recommendations.Any())
            {
                Console.WriteLine("🚨 Flask'tan boş öneri listesi geldi.");
                return View("RecommendMovies", new List<Movie>());
            }

            Console.WriteLine("🎬 Flask'tan gelen öneriler: " + string.Join(", ", result.Recommendations));

            var recommendedMovies = _context.Movies
                .Where(m => result.Recommendations.Select(r => r.ToLower()).Contains(m.Title.ToLower()))
                .ToList();

            return View("RecommendMovies", recommendedMovies ?? new List<Movie>());
      
        }




        public class RecommendationResponse
        {
            public List<string> Recommendations { get; set; }
        }


    }
}
