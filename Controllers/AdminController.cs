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
    private readonly DataContext context;

    public AdminController(IHttpContextAccessor httpContextAccessor, DataContext context)
    {
        HttpContextAccessor = httpContextAccessor;
        this.context = context;
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
        
        var users = this.context.Users
            .Where(u => u.Admin == false && u.Subscribed == true)
            .Select(u => new UserViewModel
            {
                Email = u.Email
            }).ToList();
        
        return View(users);
    }

    [AdminOnly]
    public IActionResult CreateNewsletter()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }
        
        //var newsletters = this.context.Newsletters;
        object? newsletters = null;
        
        return View(newsletters);
    }
}