using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadAndWatch.Data;
using ReadAndWatch.Models;
using System.Security.Claims;

namespace ReadAndWatch.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public BookController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Index(string selectedCategory, int? minPages, int? maxPages, int? minYear, int? maxYear)
        {
            var query = _context.Books.AsQueryable();

            var categories = _context.Books
                .Where(b => b.Categories != null)
                .Select(b => b.Categories)
                .Distinct()
                .OrderBy(c => c)
                .ToList();
            ViewBag.Categories = categories;

            if (!string.IsNullOrEmpty(selectedCategory))
                query = query.Where(b => b.Categories.Contains(selectedCategory));

            if (minPages.HasValue)
                query = query.Where(b => b.NumPages >= minPages.Value);

            if (maxPages.HasValue)
                query = query.Where(b => b.NumPages <= maxPages.Value);

            if (minYear.HasValue)
                query = query.Where(b => b.PublishedYear >= minYear.Value);

            if (maxYear.HasValue)
                query = query.Where(b => b.PublishedYear <= maxYear.Value);

            var books = query.OrderByDescending(b => b.AverageRating).ToList();
            return View(books);
        }



        [HttpPost]
        public IActionResult AddFavoriteBook(int bookId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "User");

            if (int.TryParse(userIdClaim.Value, out int userId))
            {
                var exists = _context.FavoriteBooks.FirstOrDefault(f => f.UserId == userId && f.BookId == bookId);
                if (exists == null)
                {
                    _context.FavoriteBooks.Add(new FavoriteBook { UserId = userId, BookId = bookId });
                    _context.SaveChanges();
                }
                return RedirectToAction("Favorites", "User");
            }

            return Unauthorized();
        }





        public IActionResult Details(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }




        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Recommend()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var favoriteIds = _context.FavoriteBooks
                .Where(f => f.UserId == userId)
                .Select(f => f.BookId)
                .ToList();

            Console.WriteLine("⭐ Favori ID'ler: " + string.Join(", ", favoriteIds));

            if (!favoriteIds.Any())
                return View("RecommendBooks", new List<Book>());

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/recommend", new { favorite_ids = favoriteIds });

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("🚨 Flask API başarısız yanıt döndürdü.");
                return View("RecommendBooks", new List<Book>());
            }

            var result = await response.Content.ReadFromJsonAsync<RecommendationResponse>();

            if (result?.Recommendations == null || !result.Recommendations.Any())
            {
                Console.WriteLine("🚨 Flask'tan boş öneri listesi geldi.");
                return View("RecommendBooks", new List<Book>());
            }

            Console.WriteLine("📨 Flask'tan gelen öneriler: " + string.Join(", ", result.Recommendations));

            var recommendedBooks = _context.Books
                .Where(b => result.Recommendations
                    .Select(r => r.ToLower())
                    .Contains(b.Title.ToLower()))
                .ToList();

            return View("RecommendBooks", recommendedBooks ?? new List<Book>());

        }







        public class RecommendationResponse
        {
            public List<string> Recommendations { get; set; }
        }
    }
}
