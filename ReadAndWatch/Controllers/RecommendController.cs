using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadAndWatch.Data;
using ReadAndWatch.Models;
using ReadAndWatch.Services;
using System.Linq;
using System.Security.Claims;

namespace ReadAndWatch.Controllers
{
    public class RecommendController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RecommendationService _recommendationService;

        public RecommendController(ApplicationDbContext context, RecommendationService recommendationService)
        {
            _context = context;
            _recommendationService = recommendationService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetRecommendation()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult GetRecommendation(string recommendationType)
        {
            if (recommendationType == "book")
                return RedirectToAction("RecommendBooks");
            else if (recommendationType == "movie")
                return RedirectToAction("RecommendMovies");

            return RedirectToAction("GetRecommendation");
        }


        [Authorize]
        public async Task<IActionResult> RecommendBooks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var favoriteTitles = (from fb in _context.FavoriteBooks
                                  join b in _context.Books on fb.BookId equals b.Id
                                  where fb.UserId == userId
                                  select b.Title).ToList();

            var recommendedBooks = await _recommendationService.GetBookRecommendationsByTitlesAsync(userId, favoriteTitles);

            return View("RecommendBooks", recommendedBooks);
        }


        [Authorize]
        public IActionResult RecommendMovies()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var favoriteTitles = _context.FavoriteMovies
                .Where(f => f.UserId == userId)
                .Include(f => f.Movie)
                .Select(f => f.Movie.Title)
                .ToList();

            var recommendedMovies = _recommendationService.GetMovieRecommendationsByTitlesAsync(userId, favoriteTitles);

            return View("RecommendationResults", recommendedMovies);
        }





        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CrossRecommend(string recommendationType)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            List<int> favoriteIds;
            string currentType;

            if (recommendationType == "movie") // Kullanıcının kitaplarıyla film öner
            {
                favoriteIds = await _context.FavoriteBooks
                    .Where(f => f.UserId == userId)
                    .Select(f => f.BookId)
                    .ToListAsync();
                currentType = "book";
            }
            else if (recommendationType == "book") // Kullanıcının filmleriyle kitap öner
            {
                favoriteIds = await _context.FavoriteMovies
                    .Where(f => f.UserId == userId)
                    .Select(f => f.MovieId)
                    .ToListAsync();
                currentType = "movie";
            }
            else
            {
                return BadRequest("Geçersiz öneri türü.");
            }

            if (!favoriteIds.Any())
            {
                return View("NoRecommendations");
            }

            using var httpClient = new HttpClient();
            var requestData = new
            {
                favorite_ids = favoriteIds,
                type = currentType
            };

            var response = await httpClient.PostAsJsonAsync("http://localhost:8003/recommend_cross_favorites", requestData);

            if (response.IsSuccessStatusCode)
            {
                var results = await response.Content.ReadFromJsonAsync<List<CrossRecommendationViewModel>>();

                foreach (var item in results)
                {
                    if (item.Type == "book")
                    {
                        // 📚 Kitap verisini veritabanından çek ve Thumbnail ekle
                        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == item.Id);
                        if (book != null)
                            item.Thumbnail = book.Thumbnail;
                    }
                    else if (item.Type == "movie")
                    {
                        // 🎬 Film görselini başlıktan üret (örnek: the_dark_knight.jpg)
                        var fileName = item.Title.ToLower().Replace(" ", "_") + ".jpg";
                        item.Thumbnail = $"/images/{fileName}";
                    }
                }

                return View("CrossRecommendations", results);
            }

            return View("Error");
        }






    }
}
