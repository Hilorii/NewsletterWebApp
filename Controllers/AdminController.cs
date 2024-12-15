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
    private readonly ILogger<AdminController> _logger;

    public AdminController(IHttpContextAccessor httpContextAccessor, DataContext context, ILogger<AdminController> logger)
    {
        HttpContextAccessor = httpContextAccessor;
        _context = context;
        _logger = logger;
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

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError(string.Empty, "Tytuł i treść wiadomości są wymagane.");
            return View();
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

        try
        {
            await SendEmailsToUsersWithSendGridAsync(title, content, emailLog.Id, users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wysyłania wiadomości e-mail.");
            return StatusCode(500, "Wystąpił problem z wysyłaniem wiadomości e-mail. Skontaktuj się z administratorem.");
        }

        return RedirectToAction("SentEmails", "Admin");
    }

    private async Task SendEmailsToUsersWithSendGridAsync(string title, string content, int emailLogId, IEnumerable<User> users)
    {
        var apiKey = "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";
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("hilori.furan@wp.pl", "MailCraft");

        var tasks = users.Select(async user =>
        {
            try
            {
                // Do śledzenia kliknięć
                var trackingClickUrl = Url.Action("TrackClick", "Admin", new { logId = emailLogId }, Request.Scheme);

                // Do śledzenia otwarć
                var trackingOpenUrl = Url.Action("TrackOpen", "Admin", new { logId = emailLogId }, Request.Scheme);

                // Treść maila z śledzącymi linkami
                var htmlContent = $@"
                {content}
                <br><br>
                <a href=""{trackingClickUrl}"">Kliknij tutaj</a>
                <br>
                <img src=""{trackingOpenUrl}"" alt="""" style=""display:none;"">";

                var to = new EmailAddress(user.Email);
                var msg = MailHelper.CreateSingleEmail(from, to, title, content, htmlContent);

                await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Błąd wysyłania e-maila do {user.Email}");
            }
        });

        await Task.WhenAll(tasks);
    }

    
    [HttpGet]
    public IActionResult TrackOpen(int logId)
    {
        var emailLog = _context.EmailLogs.Find(logId);
        if (emailLog == null)
        {
            return NotFound();
        }

        var emailOpen = new EmailOpen
        {
            EmailLogId = logId,
            CreatedAt = DateTime.UtcNow
        };
        _context.EmailOpens.Add(emailOpen);
        _context.SaveChanges();

        // Zwraca pusty obrazek 1x1 jako odpowiedź (tracking pixel)
        return File(new byte[0], "image/gif");
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

        var emails = _context.Emails
            .Select(e => new EmailViewModel
            {
                Title = e.Title,
                SentAt = e.EmailLogs
                    .OrderByDescending(el => el.SentAt)
                    .FirstOrDefault().SentAt,
                TotalClicks = e.EmailLogs
                    .SelectMany(el => el.Clicks)
                    .Count(),
                TotalOpens = e.EmailLogs
                    .SelectMany(el => _context.EmailOpens.Where(op => op.EmailLogId == el.Id))
                    .Count()
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

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError(string.Empty, "Tytuł i treść wiadomości są wymagane.");
            return View();
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
