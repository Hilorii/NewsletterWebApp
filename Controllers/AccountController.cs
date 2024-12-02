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

        //LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //LOGIN
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("Email", user.Email);
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("IsAdmin", user.Admin ? "true" : "false");

                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.ErrorMessage = "Nieprawidłowy email lub hasło.";
            return View();
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        //REJESTRACJA
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //REJESTRACJA POST
        [HttpPost]
        public IActionResult Register(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.ErrorMessage = "Email i hasło są wymagane.";
                return View();
            }
            
            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.ErrorMessage = "Podany email jest już zarejestrowany.";
                return View();
            }
            var user = new User
            {
                Email = email,
                Password = password, 
                Admin = false  
            };
            
            _context.Users.Add(user);
            _context.SaveChanges();
            
            return RedirectToAction("Login", "Account");
        }
    }
}