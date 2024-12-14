using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewsletterWebApp.Models;
using NewsletterWebApp.Data;
using NewsletterWebApp.ViewModels;
using Microsoft.AspNetCore.Http;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Linq;

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

        var users = _context.Users
            .Where(u => !u.Admin)
            .Select(u => new UserViewModel
            {
                Email = u.Email
            }).ToList();
              
        return View(users);
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult SendEmail(string[] emailAddresses, string title, string content)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        // TODO: obsłużyć faktyczne wysłanie wiadomości

        var email = new Email
        {
            Title = title,
            Content = content
        };
        _context.Emails.Add(email);
        _context.SaveChanges(); // konieczne zapisanie od razu w celu uzyskania ID e-maila

        var emailLog = new EmailLog
        {
            EmailId = email.Id
        };
        _context.EmailLogs.Add(emailLog);
        _context.SaveChanges();

        foreach (var emailAddress in emailAddresses)
        {
            var emailLogUser = new EmailLogUser
            {
                EmailLogId = emailLog.Id,
                UserId = _context.Users.Single(u => u.Email == emailAddress).Id
            };
            _context.EmailLogUsers.Add(emailLogUser);
        }
        _context.SaveChanges();

        /* // kod niewymagający wielokrotnego zapisywania danych w bazie (obecnie nie działa)
        var email = new Email
        {
            Title = title,
            Content = content
        };
        _context.Emails.Add(email);

        var emailLog = new EmailLog();
        email.EmailLogs.Add(emailLog);

        foreach (var emailAddress in emailAddresses)
        {
            var emailLogUser = new EmailLogUser
            {
                UserId = _context.Users.Single(u => u.Email == emailAddress).Id
            };
            emailLog.EmailLogUsers.Add(emailLogUser);
        }

        _context.SaveChanges();
        */

        return RedirectToAction("SendEmail", "Admin");
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

        return RedirectToAction("CreateNewsletter", "Admin");
    }
}