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
    private readonly DataContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, DataContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        // POBIERANIE DANYCH DLA WIDOKU
        var email = HttpContext.Session.GetString("Email");
        
        if (email != null)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                // WYSYŁANIE DANYCH DO WIDOKU

                var userViewModel = new UserViewModel
                {
                    Email = user.Email,
                    IsSubscribed = user.Subscribed,
                    MailingListSubscriptionIds = _context.Subscriptions
                        .Where(s => s.UserId == user.Id)
                        .Select(s => s.MailingListId)
                        .ToList(),
                    IsAdmin = user.Admin
                };
                var mailingLists = _context.MailingLists
                    .Select(l => new MailingListViewModel
                    {
                        Id = l.Id,
                        Name = l.Name
                    })
                    .ToList();

                return View(new UserAndMailingListViewModel
                {
                    User = userViewModel,
                    MailingLists = mailingLists
                });
            }
        }

        return View(new UserAndMailingListViewModel()); // pusty model
    }

    // PRZYCISK NA SUBA
    [HttpPost]
    public IActionResult SubscribeToNewsletter(int id)
    {
        var email = HttpContext.Session.GetString("Email");

        if (email != null)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                var subscription = new Subscription
                {
                    UserId = user.Id,
                    MailingListId = id
                };
                _context.Subscriptions.Add(subscription);
                _context.SaveChanges();
            }
        }

        return RedirectToAction("Index");
    }

    // PRZYCISK NA UNSUBA
    [HttpPost]
    public IActionResult UnsubscribeFromNewsletter(int id)
    {
        var email = HttpContext.Session.GetString("Email");

        if (email != null)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                var subscription = _context.Subscriptions.Single(s => s.UserId == user.Id && s.MailingListId == id);
                _context.Subscriptions.Remove(subscription);
                _context.SaveChanges();
            }
        }

        return RedirectToAction("Index");
    }

    /*
    // PRZYCISK NA SUBA
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

    // PRZYCISK NA UNSUBA
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
    */
}