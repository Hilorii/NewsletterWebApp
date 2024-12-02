using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewsletterWebApp.Models;
using System.Data;
using NewsletterWebApp.Data;
using NewsletterWebApp.ViewModels;
using Microsoft.AspNetCore.Http;

namespace NewsletterWebApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly DataContext context;

    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        this.context = context;
    }

    public IActionResult Index()
    {
        //POBIERANIE DANYCH DLA WIDOKU
        var email = HttpContext.Session.GetString("Email");
        
        if (email != null)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                ViewBag.Email = email;
                ViewBag.IsSubscribed = user.Subscribed;
                ViewBag.IsAdmin = user.Admin;
            }
        }
        
        //WYSYŁANIE DANYCH DO WIDOKU
        ViewBag.Email = email;
        
        return View();
    }
    //PRZYCISK NA SUBA
    [HttpPost]
    public IActionResult SubscribeToNewsletter()
    {
        var email = HttpContext.Session.GetString("Email");

        if (email != null)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.Subscribed = true;
                context.SaveChanges();
            }
        }

        return RedirectToAction("Index");
    }
    //PRZYCISK NA UNSUBA
    [HttpPost]
    public IActionResult UnsubscribeFromNewsletter()
    {
        var email = HttpContext.Session.GetString("Email");

        if (email != null)
        {
            var user = context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                user.Subscribed = false;
                context.SaveChanges();
            }
        }

        return RedirectToAction("Index");
    }
    
    
    
}