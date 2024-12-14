using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using NewsletterWebApp.Models;
using NewsletterWebApp.Data;
using NewsletterWebApp.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;

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
    [HttpPost]
    public async Task<IActionResult> SendEmail(string title, string content)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var users = _context.Users
            .Where(u => !u.Admin && u.Subscribed)
            .ToList();

        var email = new Email
        {
            Title = title,
            Content = content
        };
        _context.Emails.Add(email);
        _context.SaveChanges();

        var emailLog = new EmailLog
        {
            EmailId = email.Id
        };
        _context.EmailLogs.Add(emailLog);
        _context.SaveChanges();

        foreach (var user in users)
        {
            var emailLogUser = new EmailLogUser
            {
                EmailLogId = emailLog.Id,
                UserId = user.Id
            };
            _context.EmailLogUsers.Add(emailLogUser);
        }
        _context.SaveChanges();

        await SendEmailsToUsersWithSendGridAsync(title, content, emailLog.Id, users);

        return RedirectToAction("SentEmails", "Admin");
    }

    private async Task SendEmailsToUsersWithSendGridAsync(string title, string content, int emailLogId, IEnumerable<User> users)
    {
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("hilori.furan@wp.pl", "MailGrid");

        foreach (var user in users)
        {
            var trackingUrl = Url.Action("TrackClick", "Admin", new { logId = emailLogId }, Request.Scheme);
            var htmlContent = $"{content}<br><br><a href=\"{trackingUrl}\">Kliknij tutaj</a>";

            var to = new EmailAddress(user.Email);
            var msg = MailHelper.CreateSingleEmail(from, to, title, content, htmlContent);

            await client.SendEmailAsync(msg);
        }
    }

    private string GenerateEmailContent(string content, int emailLogId)
    {
        var trackingUrl = Url.Action("TrackClick", "Admin", new { logId = emailLogId }, Request.Scheme);
        return $"{content}<br><br><a href=\"{trackingUrl}\">Kliknij tutaj</a>";
    }

    [HttpGet]
    public IActionResult TrackClick(int logId)
    {
        var emailLog = _context.EmailLogs.Find(logId);
        if (emailLog == null)
        {
            return NotFound();
        }

        var click = new Click
        {
            EmailLogId = logId
        };
        _context.Clicks.Add(click);
        _context.SaveChanges();

        return Redirect("/");
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
