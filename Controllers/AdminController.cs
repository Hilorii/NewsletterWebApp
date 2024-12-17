using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
    
    [HttpPost]
    public async Task<IActionResult> SendEmail(string title, string content, DateTime? scheduledAt)
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
            ScheduledAt = scheduledAt.HasValue ? scheduledAt.Value.ToUniversalTime() : DateTime.UtcNow

        };

        _context.Emails.Add(email);
        _context.SaveChanges();

        if (!scheduledAt.HasValue || scheduledAt <= DateTime.UtcNow)
        {
            var users = _context.Users
                .Where(u => !u.Admin && u.Subscribed)
                .ToList();

            try
            {
                await SendEmailsToUsersWithSendGridAsync(title, content, email.Id, users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wysyłania wiadomości e-mail.");
                return StatusCode(500, "Wystąpił problem z wysyłaniem wiadomości e-mail. Skontaktuj się z administratorem.");
            }
        }

        return RedirectToAction("SentEmails", "Admin");
    }


    
    
    //TEMPLATE DLA MAILI 
    private string ProcessTemplate(string template, User user)
    {
        return template
            .Replace("{{Email}}", user.Email);
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
                // Przetwórz tytuł i treść z dynamicznymi zmiennymi
                var personalizedTitle = ProcessTemplate(title, user);
                var personalizedContent = ProcessTemplate(content, user);

                // Link śledzący kliknięcia
                var trackingUrl = Url.Action("TrackClick", "Admin", new { logId = emailLogId }, Request.Scheme);

                // Link do piksela śledzącego otwarcia
                var pixelUrl = Url.Action("TrackOpen", "Admin", new { logId = emailLogId }, Request.Scheme);

                // Dodaj piksel do treści wiadomości
                var htmlContent = $"{personalizedContent}<br><br><a href=\"{trackingUrl}\">Kliknij tutaj</a>" +
                                  $"<img src=\"{pixelUrl}\" alt=\"\" style=\"display:none;\" />";

                // Wyślij email
                var to = new EmailAddress(user.Email);
                var msg = MailHelper.CreateSingleEmail(from, to, personalizedTitle, personalizedContent, htmlContent);

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

        var open = new EmailOpen
        {
            EmailLogId = logId,
            CreatedAt = DateTime.UtcNow
        };
        _context.EmailOpens.Add(open);
        _context.SaveChanges();

        // Zwróć pusty obraz
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
    
    //Wysyłanie maili zaplanowanych
    public async Task SendScheduledEmails()
    {
        _logger.LogInformation("Rozpoczynam wysyłanie zaplanowanych e-maili.");

        // Pobierz e-maile do wysłania
        var emailsToSend = _context.Emails
            .Where(e => e.ScheduledAt.HasValue && e.ScheduledAt.Value.ToUniversalTime() <= DateTime.UtcNow && !e.IsSent)
            .ToList();

        _logger.LogInformation($"Znaleziono {emailsToSend.Count} zaplanowanych e-maili do wysłania.");

        var users = _context.Users
            .Where(u => !u.Admin && u.Subscribed)
            .ToList();

        foreach (var email in emailsToSend)
        {
            try
            {
                // Log wysyłki
                _logger.LogInformation($"Wysyłanie e-maila: {email.Title}, ID: {email.Id}");

                await SendEmailsToUsersWithSendGridAsync(email.Title, email.Content, email.Id, users);
                
                email.IsSent = true;
                _context.Emails.Update(email);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Błąd podczas wysyłania e-maila ID: {email.Id}");
            }
        }

        try
        {
            _context.SaveChanges();
            _logger.LogInformation("Zapisano zmiany w bazie danych po wysyłce e-maili.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas zapisywania zmian w bazie danych.");
        }
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
            .Include(e => e.EmailLogs)
            .ThenInclude(el => el.Clicks)
            .ToList()
            .Select(e => new EmailViewModel
            {
                Title = e.Title ?? "Brak tytułu",
                SentAt = e.EmailLogs?
                    .OrderByDescending(el => el.SentAt)
                    .FirstOrDefault()?.SentAt ?? DateTime.MinValue,
                TotalClicks = e.EmailLogs?
                    .SelectMany(el => el.Clicks)
                    .Count() ?? 0,
                TotalOpens = e.EmailLogs?
                    .SelectMany(el => _context.EmailOpens.Where(op => op.EmailLogId == el.Id))
                    .Count() ?? 0
            })
            .ToList();


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
