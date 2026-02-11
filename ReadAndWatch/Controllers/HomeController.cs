using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReadAndWatch.Models;
using ReadAndWatch.Data;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ReadAndWatch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Trend filmleri vote_average'a göre sýrala
            var trendMovies = _context.Movies
                .Where(m => m.VoteAverage != null)
                .OrderByDescending(m => m.VoteAverage)
                .Take(6)
                .ToList();

            // Trend kitaplarý average_rating'e göre sýrala
            var trendBooks = _context.Books
                .Where(b => b.AverageRating != null)
                .OrderByDescending(b => b.AverageRating)
                .Take(6)
                .ToList();

            // Kitaplarý ViewBag ile taþý
            ViewBag.TrendBooks = trendBooks;

            return View(trendMovies);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpGet]
        public JsonResult SearchJson(string query)
        {
            var books = _context.Books
                .Where(b => b.Title.Contains(query))
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Authors,
                    b.Thumbnail
                }).ToList();

            var movies = _context.Movies
                .Where(m => m.Title.Contains(query))
                .Select(m => new
                {
                    m.Id,
                    m.Title,
                    m.Overview
                }).ToList();

            return Json(new { books, movies });
        }


    }
}
