using Microsoft.AspNetCore.Mvc;
using NewsletterWebApp.Data;
using System.Linq; // Dla LINQ
using Microsoft.AspNetCore.Http; // Dla sesji

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
            // Wyczyść sesję
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}