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
        public IActionResult Register(string username, string password)
        {
            // Logic to save the user in the database (mocked)
            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction("Login");
        }
    }
}