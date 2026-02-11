using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadAndWatch.Models;
using ReadAndWatch.Data;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ReadWatch.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /User/Register
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                _context.Users.Add(user);
                int result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    TempData["Message"] = "Kayıt başarıyla gerçekleşti.";
                    return RedirectToAction("Login", "User");
                }
                else
                {
                    TempData["Error"] = "Kayıt başarısız! Veritabanına yazılamadı.";
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Hata oluştu: " + ex.Message;
                return View(user);
            }
        }







        // GET: /User/Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user != null)
            {
                // 1. Claims oluştur
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };

                // 2. Claims Identity oluştur
                var identity = new ClaimsIdentity(claims, "Cookies");
                var principal = new ClaimsPrincipal(identity);

                // 3. ASP.NET'e kullanıcıyı tanıt
                await HttpContext.SignInAsync("Cookies", principal);

                // 4. (İsteğe bağlı) Session da devam etsin dersen:
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Geçersiz e-posta veya şifre";
            return View();
        }




        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            HttpContext.Session.Clear(); // İsteğe bağlı
            return RedirectToAction("Login", "User");
        }




        [Authorize]
        public IActionResult Favorites()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                // Kullanıcı ID alınamazsa giriş sayfasına yönlendir
                return RedirectToAction("Login", "User");
            }

            var favoriteBooks = _context.FavoriteBooks
                .Where(f => f.UserId == userId)
                .Select(f => f.Book)
                .ToList();

            var favoriteMovies = _context.FavoriteMovies
                .Where(f => f.UserId == userId)
                .Select(f => f.Movie)
                .ToList();

            var viewModel = new FavoriteViewModel
            {
                FavoriteBooks = favoriteBooks,
                FavoriteMovies = favoriteMovies
            };

            return View("MyFavorites", viewModel);
        }



        [HttpPost]
        [Authorize]
        public IActionResult RemoveFavoriteBook(int bookId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "User");
            }

            var favorite = _context.FavoriteBooks
                .FirstOrDefault(f => f.UserId == userId && f.BookId == bookId);

            if (favorite != null)
            {
                _context.FavoriteBooks.Remove(favorite);
                _context.SaveChanges();
            }

            return RedirectToAction("Favorites");
        }

        [HttpPost]
        [Authorize]
        public IActionResult RemoveFavoriteMovie(int movieId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
            {
                return RedirectToAction("Login", "User");
            }

            var favorite = _context.FavoriteMovies
                .FirstOrDefault(f => f.UserId == userId && f.MovieId == movieId);

            if (favorite != null)
            {
                _context.FavoriteMovies.Remove(favorite);
                _context.SaveChanges();
            }

            return RedirectToAction("Favorites");
        }







        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
                return RedirectToAction("Login");

            return View();
        }

    }
}
