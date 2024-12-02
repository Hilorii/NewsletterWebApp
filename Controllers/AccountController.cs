using Microsoft.AspNetCore.Mvc;
using NewsletterWebApp.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace NewsletterWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;

        public AccountController(DataContext context)
        {
            _context = context;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Znajdź użytkownika na podstawie email i hasła
            var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Ustawienie danych w sesji
                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("IsAdmin", user.Admin ? "true" : "false");

                return RedirectToAction("Index", "Home");
            }

            // Błąd logowania
            ViewBag.ErrorMessage = "Nieprawidłowy email lub hasło.";
            return View();
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        // GET: Registration Page
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register User
        [HttpPost]
        public IActionResult Register(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Email i hasło są wymagane.";
                return View();
            }

            // Sprawdzenie, czy użytkownik już istnieje
            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.ErrorMessage = "Podany email jest już zarejestrowany.";
                return View();
            }

            // Tworzenie nowego użytkownika
            var user = new User
            {
                Email = email,
                Password = password, // W produkcji zahaszuj hasło
                Admin = false      // Domyślnie nowi użytkownicy nie są adminami
            };

            // Zapis do bazy danych
            _context.Users.Add(user);
            _context.SaveChanges();

            // Przekierowanie do logowania
            return RedirectToAction("Login", "Account");
        }
    }
}