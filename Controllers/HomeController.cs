using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewsletterWebApp.Models;
using System.Data;
using NewsletterWebApp.Data;
using NewsletterWebApp.ViewModels;

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
        var users = this.context.Users.Select(u => new UserViewModel
        {
            Email = u.Email
        });
        return View(users);
    }

    public IActionResult Privacy()
    {
        return View();
    }  

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
}