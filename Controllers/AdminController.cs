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
using NewsletterWebApp.Migrations;

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
        return HttpContextAccessor.HttpContext?.Session?.GetString("IsAdmin") == "true";
    }

    [HttpPost]
    public async Task<IActionResult> SendEmail(string title, string content, DateTime? scheduledAt, bool scheduled, bool isAdminOverride = false)
    {
        if (!isAdminOverride && !IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError(string.Empty, "Tytuł i treść wiadomości są wymagane.");
            return View();
        }

        if (scheduledAt.HasValue && scheduledAt > DateTime.UtcNow)
        {
            scheduled = true;
        }

        Email email;

        if (isAdminOverride && scheduled)
        {
            // Retrieve the existing email instead of creating a new one
            email = _context.Emails.FirstOrDefault(e => e.Title == title && e.Content == content && e.ScheduledAt == scheduledAt);
            if (email == null)
            {
                return StatusCode(404, "Nie znaleziono wiadomości e-mail do wysłania.");
            }
        }
        else
        {
            // Create a new email record
            email = new Email
            {
                Title = title,
                Content = content,
                ScheduledAt = scheduledAt ?? DateTime.UtcNow,
                IsScheduled = scheduled
            };

            _context.Emails.Add(email);
            _context.SaveChanges();
        }

        if (scheduled)
        {
            if (scheduledAt.HasValue && scheduledAt <= DateTime.UtcNow)
            {
                var users = _context.Users
                    .Where(u => !u.Admin && u.Subscribed)
                    .ToList();

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

                    email.IsSent = true;
                    email.IsScheduled = false;
                    email.UpdatedAt = DateTime.UtcNow;
                    _context.Emails.Update(email);
                    await _context.SaveChangesAsync();
                    await SendEmailsToUsersWithSendGridAsync(title, content, email.Id, users);
                    _logger.LogInformation("Email {EmailId} sent and status updated.", email.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wysyłania wiadomości e-mail.");
                    return StatusCode(500, "Wystąpił problem z wysyłaniem wiadomości e-mail. Skontaktuj się z administratorem.");
                }
            }
        }
        else
        {
            var users = _context.Users
                .Where(u => !u.Admin && u.Subscribed)
                .ToList();

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

            if (!scheduledAt.HasValue || scheduledAt <= DateTime.UtcNow)
            {
                try
                {
                    email.IsSent = true;
                    email.IsScheduled = false;
                    email.UpdatedAt = DateTime.UtcNow;
                    _context.Emails.Update(email);
                    await _context.SaveChangesAsync();
                    await SendEmailsToUsersWithSendGridAsync(title, content, email.Id, users);
                    _logger.LogInformation("Email {EmailId} sent and status updated.", email.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Błąd podczas wysyłania wiadomości e-mail.");
                    return StatusCode(500, "Wystąpił problem z wysyłaniem wiadomości e-mail. Skontaktuj się z administratorem.");
                }
            }
        }

        return RedirectToAction("SentEmails", "Admin");
    }

    // TEMPLATE DLA MAILI 
    private string ProcessTemplate(string template, User user)
    {
        return template
            .Replace("{{Email}}", user.Email);
    }

    public async Task SendEmailsToUsersWithSendGridAsync(string title, string content, int emailLogId, IEnumerable<User> users)
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

                var baseUrl = "http://localhost:5189";
                var trackingUrl = $"{baseUrl}/Admin/TrackClick?logId={emailLogId}";
                var pixelUrl = $"{baseUrl}/Admin/TrackOpen?logId={emailLogId}";
                _logger.LogInformation("Generated pixel URL: {pixelUrl} for user: {email}", pixelUrl, user.Email);

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

    // public async Task SendEmailsToUsersWithSendGridAsync(string title, string content, int emailLogId, IEnumerable<User> users)
    // {
    //     var apiKey = "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";
    //     var client = new SendGridClient(apiKey);
    //     var from = new EmailAddress("hilori.furan@wp.pl", "MailCraft");
    //
    //     var tasks = users.Select(async user =>
    //     {
    //         try
    //         {
    //             // Przetwórz tytuł i treść z dynamicznymi zmiennymi
    //             var personalizedTitle = ProcessTemplate(title, user);
    //             var personalizedContent = ProcessTemplate(content, user);
    //
    //             // Link śledzący kliknięcia
    //             
    //
    //      
    //             
    //             // Dodaj piksel do treści wiadomości
    //             var htmlContent = $"{personalizedContent}<br><br><a href=\"\">Kliknij tutaj</a>" +
    //                               $"<img src=\"\" alt=\"\" style=\"display:none;\" />";
    //
    //             // Wyślij email
    //             var to = new EmailAddress(user.Email);
    //             var msg = MailHelper.CreateSingleEmail(from, to, personalizedTitle, personalizedContent, htmlContent);
    //
    //             await client.SendEmailAsync(msg);
    //         }
    //         catch (Exception ex)
    //         {
    //             _logger.LogError(ex, $"Błąd wysyłania e-maila do {user.Email}");
    //         }
    //     });
    //
    //     await Task.WhenAll(tasks);
    // }

    [HttpGet]
    public IActionResult TrackOpen(int logId)
    {
        _logger.LogInformation("TrackOpen invoked with logId: {logId}", logId);

        var emailLog = _context.EmailLogs.FirstOrDefault(e => e.EmailId == logId);

        if (emailLog == null)
        {
            _logger.LogWarning("TrackOpen: EmailLog not found for logId: {logId}", logId);
            return NotFound();
        }

        var open = new EmailOpen
        {
            EmailLogId = emailLog.Id
        };

        try
        {
            _context.EmailOpens.Add(open);
            _context.SaveChanges();
            _logger.LogInformation("TrackOpen: Successfully added EmailOpen entry for logId: {logId}", logId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "TrackOpen: Error saving EmailOpen entry for logId: {logId}", logId);
        }

        // Return a transparent pixel
        return File(new byte[0], "image/gif");
    }

    [HttpGet]
    public IActionResult TrackClick(int logId)
    {
        var emailLog = _context.EmailLogs.FirstOrDefault(e => e.EmailId == logId);
        if (emailLog == null)
        {
            return NotFound();
        }

        var click = new Click
        {
            EmailLogId = emailLog.Id
        };
        _context.Clicks.Add(click);
        _context.SaveChanges();

        return Redirect("/");
    }

    // Wysyłanie maili zaplanowanych
    public async Task SendScheduledEmails()
    {
        // Pobierz e-maile do wysłania
        var emailsToSend = _context.Emails
            .Where(e => e.ScheduledAt.HasValue && e.ScheduledAt.Value.ToUniversalTime() <= DateTime.UtcNow)
            .ToList();

        // Pobierz listę użytkowników
        var users = _context.Users
            .Where(u => !u.Admin && u.Subscribed)
            .ToList();

        foreach (var email in emailsToSend)
        {
            try
            {
                // Wyślij zaplanowane e-maile
                await SendEmailsToUsersWithSendGridAsync(email.Title, email.Content, email.Id, users);

                // Resetowanie daty zaplanowanej
                email.ScheduledAt = null;
                _context.Emails.Update(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Błąd podczas wysyłania zaplanowanego e-maila: {email.Title}, ID: {email.Id}");
            }
        }

        try
        {
            // Zapisz zmiany w bazie danych
            _context.SaveChanges();
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
    public IActionResult SendEmail(int? id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var email = _context.Emails.SingleOrDefault(n => n.Id == id);

        return View(new EmailViewModel
        {
            Title = email?.Title ?? "",
            Content = email?.Content ?? ""
        });
    }

    [AdminOnly]
    public IActionResult SentEmails()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var emails = _context.Emails
            .Where(e => !e.IsNewsletter && e.IsSent)
            .Include(e => e.EmailLogs)
            .ThenInclude(el => el.Clicks)
            .Include(e => e.EmailLogs)
            .ThenInclude(el => el.EmailLogUsers)
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
                    .SelectMany(el => _context.EmailOpens
                        .Where(op => op.EmailLogId == el.Id))
                    .Count() ?? 0
            })
            .ToList();

        return View(emails);
    }

    [AdminOnly]
    public IActionResult PlannedEmails()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var emails = _context.Emails
            .Where(e => !e.IsNewsletter && !e.IsSent)
            .Include(e => e.EmailLogs)
            .ToList()
            .Select(e => new EmailViewModel
            {
                Id = e.Id,
                Title = e.Title ?? "Brak tytułu",
                ScheduledAt = e.ScheduledAt ?? DateTime.MinValue
            })
            .ToList();

        return View(emails);
    }

    // USUWANIE ZAPLANOWANYCH MAILÓW
    [HttpPost]
    public IActionResult DeletePlannedEmail(int id)
    {
        var email = _context.Emails.FirstOrDefault(e => e.Id == id);
        if (email == null)
        {
            return NotFound();
        }

        _context.Emails.Remove(email);
        _context.SaveChanges();

        return RedirectToAction("PlannedEmails");
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
                Id = n.Id,
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

        var mailingLists = _context.MailingLists
            .Select(ml => new MailingListViewModel
            {
                Id = ml.Id,
                Name = ml.Name
            })
            .ToList();

        return View(mailingLists);
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult CreateNewsletter(string title, string content, string? imageUrl, int[] mailingListIds)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError(string.Empty, "Tytuł i treść wiadomości są wymagane.");

            var mailingLists = _context.MailingLists
                .Select(ml => new MailingListViewModel
                {
                    Id = ml.Id,
                    Name = ml.Name
                })
                .ToList();

            return View(mailingLists);
        }

        var newsletter = new Email
        {
            Title = title,
            Content = content,
            ImageUrl = imageUrl,
            IsNewsletter = true
        };
        _context.Emails.Add(newsletter);
        _context.SaveChanges();

        var newsletterId = newsletter.Id;
        foreach (var mailingListId in mailingListIds)
        {
            var emailMailingList = new EmailMailingList
            {
                EmailId = newsletterId,
                MailingListId = mailingListId
            };
            _context.EmailMailingLists.Add(emailMailingList);
        }
        _context.SaveChanges();

        return RedirectToAction("NewslettersList", "Admin");
    }

    [AdminOnly]
    public IActionResult EditNewsletter(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var newsletter = _context.Emails
            .Include(e => e.EmailMailingLists)
            .Single(n => n.IsNewsletter && n.Id == id);

        var newsletterModel = new NewsletterViewModel
        {
            Id = newsletter.Id,
            Title = newsletter.Title,
            Content = newsletter.Content,
            MailingListIds = newsletter.EmailMailingLists.Select(eml => eml.MailingListId).ToList()
        };
        var mailingLists = _context.MailingLists
            .Select(ml => new MailingListViewModel
            {
                Id = ml.Id,
                Name = ml.Name
            })
            .ToList();
        return View(new NewsletterAndMailingListViewModel
        {
            Newsletter = newsletterModel,
            MailingLists = mailingLists
        });
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult EditNewsletter(int id, string title, string content, string? imageUrl, int[] mailingListIds)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var newsletter = _context.Emails.Single(n => n.IsNewsletter && n.Id == id);

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            ModelState.AddModelError(string.Empty, "Tytuł i treść wiadomości są wymagane.");

            var newsletterModel = new NewsletterViewModel
            {
                Id = newsletter.Id,
                Title = newsletter.Title,
                Content = newsletter.Content,
                MailingListIds = newsletter.EmailMailingLists.Select(eml => eml.MailingListId).ToList()
            };
            var mailingLists = _context.MailingLists
                .Select(ml => new MailingListViewModel
                {
                    Id = ml.Id,
                    Name = ml.Name
                })
                .ToList();
            return View(new NewsletterAndMailingListViewModel
            {
                Newsletter = newsletterModel,
                MailingLists = mailingLists
            });
        }

        _context.Emails.Update(newsletter);
        newsletter.Title = title;
        newsletter.Content = content;
        newsletter.ImageUrl = imageUrl;

        foreach (var mailingListId in mailingListIds)
        {
            var emailMailingList = new EmailMailingList
            {
                EmailId = id,
                MailingListId = mailingListId
            };
            if (!_context.EmailMailingLists.Contains(emailMailingList))
            {
                _context.EmailMailingLists.Add(emailMailingList);
            }
        }
        foreach (var emailMailingList in _context.EmailMailingLists.Where(eml => eml.EmailId == id))
        {
            if (!mailingListIds.ToList().Contains(emailMailingList.MailingListId))
            {
                _context.EmailMailingLists.Remove(emailMailingList);
            }
        }

        _context.SaveChanges();

        return RedirectToAction("NewslettersList", "Admin");
    }

    [AdminOnly]
    public IActionResult DeleteNewsletter(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var newsletter = _context.Emails.Single(n => n.IsNewsletter && n.Id == id);

        return View(new NewsletterViewModel
        {
            Id = newsletter.Id,
            Title = newsletter.Title
        });
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult DeleteNewsletter(int id, bool confirm)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        if (confirm)
        {
            var email = _context.Emails.Single(n => n.IsNewsletter && n.Id == id);
            _context.Emails.Remove(email);
            _context.SaveChanges();
        }

        return RedirectToAction("NewslettersList", "Admin");
    }

    [AdminOnly]
    public IActionResult MailingLists()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var mailingLists = _context.MailingLists
            .Select(l => new MailingListViewModel
            {
                Id = l.Id,
                Name = l.Name
            })
            .ToList();

        return View(mailingLists);
    }

    [AdminOnly]
    public IActionResult CreateMailingList()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        return View();
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult CreateMailingList(string name)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(string.Empty, "Nazwa listy jest wymagana.");
            return View();
        }

        var mailingList = new MailingList
        {
            Name = name
        };
        _context.MailingLists.Add(mailingList);
        _context.SaveChanges();

        return RedirectToAction("MailingLists", "Admin");
    }

    public IActionResult EditMailingList(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var mailingList = _context.MailingLists.Single(l => l.Id == id);

        return View(new MailingListViewModel
        {
            Id = mailingList.Id,
            Name = mailingList.Name
        });
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult EditMailingList(int id, string name)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var mailingList = _context.MailingLists.Single(l => l.Id == id);

        if (string.IsNullOrWhiteSpace(name))
        {
            ModelState.AddModelError(string.Empty, "Tytuł i treść wiadomości są wymagane.");
            return View(new MailingListViewModel
            {
                Id = mailingList.Id,
                Name = mailingList.Name
            });
        }

        _context.MailingLists.Update(mailingList);
        mailingList.Name = name;
        _context.SaveChanges();

        return RedirectToAction("MailingLists", "Admin");
    }

    [AdminOnly]
    public IActionResult DeleteMailingList(int id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var mailingList = _context.MailingLists.Single(l => l.Id == id);

        return View(new MailingListViewModel
        {
            Id = mailingList.Id,
            Name = mailingList.Name
        });
    }

    [AdminOnly]
    [HttpPost]
    public IActionResult DeleteMailingList(int id, bool confirm)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        if (confirm)
        {
            var mailingList = _context.MailingLists.Single(l => l.Id == id);
            _context.MailingLists.Remove(mailingList);
            _context.SaveChanges();
        }

        return RedirectToAction("MailingLists", "Admin");
    }
}
