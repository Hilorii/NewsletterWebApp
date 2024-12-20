using Microsoft.EntityFrameworkCore;
using NewsletterWebApp.Controllers;
using NewsletterWebApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Dodanie usług MVC
builder.Services.AddControllersWithViews();

// Konfiguracja bazy danych PostgreSQL
builder.Services.AddDbContext<DataContext>(
    o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Konfiguracja pamięci podręcznej dla sesji
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Sesja wygasa po 30 minutach
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Dodanie kontekstu HTTP i kontrolerów
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AdminController>();

// Dla zaplanowanych maili
// builder.Services.AddHostedService<ScheduledEmailSender>();

var app = builder.Build();

// obsluga błędów
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware sesji
app.UseSession();

// Middleware HTTPS i plików statycznych
app.UseHttpsRedirection();
app.UseStaticFiles();


// Konfiguracja routingu
app.UseRouting();
app.UseAuthorization();

// Trasy aplikacji
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// app.MapControllerRoute(
//     name: "admin",
//     pattern: "Admin/{action=SubscribersList}/{id?}",
//     defaults: new { controller = "Admin" });

app.Run();
