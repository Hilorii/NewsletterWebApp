using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewsletterWebApp.Models;
using System.Data;
using NewsletterWebApp.Data;
using NewsletterWebApp.ViewModels;
using Microsoft.AspNetCore.Http;

namespace NewsletterWebApp.Controllers;
public class AdminController : Controller
{
    private readonly DataContext _context;

    public AdminController(IHttpContextAccessor httpContextAccessor, DataContext context)
    {
        HttpContextAccessor = httpContextAccessor;
        _context = context;
    }

    private IHttpContextAccessor HttpContextAccessor { get; }

    private bool IsAdmin()
    {
        return HttpContextAccessor.HttpContext.Session.GetString("IsAdmin") == "true";
    }

    [AdminOnly]
    public IActionResult SubscribersList()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }
        
        var users = _context.Users
            .Where(u => !u.Admin && u.Subscribed)
            .Select(u => new UserViewModel
            {
                Email = u.Email
            }).ToList();
        
        return View(users);
    }

    [AdminOnly]
    public IActionResult SendEmail()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }
              
        return View();
    }

    [AdminOnly]
    public IActionResult SentEmails()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }


        var emails = _context.EmailLogs
            .Join(_context.Emails, el => el.EmailId, e => e.Id, (el, e) => new EmailViewModel
            {
                Title = e.Title,
                SentAt = el.SentAt
            }).ToList();
        
        return View(emails);
    }

    [AdminOnly]
    public IActionResult NewslettersList()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }
        
        var newsletters = _context.Emails
            .Where(n => n.IsNewsletter)
            .Select(n => new NewsletterViewModel
            {
                Title = n.Title
            }).ToList();
        
        return View(newsletters);
    }

    [AdminOnly]
    public IActionResult CreateNewsletter()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }
              
        return View();
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult CreateNewsletter(string title, string content, string? imageUrl)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var email = new Email
        {
            Title = title,
            Content = content,
            ImageUrl = imageUrl,
            IsNewsletter = true
        };

        _context.Emails.Add(email);
        _context.SaveChanges();

        return View();
    }
}