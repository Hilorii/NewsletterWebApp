using Microsoft.EntityFrameworkCore;
using NewsletterWebApp.Data;
using NewsletterWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DataContext>(
    o => o.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tyle trwa sesja!
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=SubscribersList}/{id?}",
    defaults: new { controller = "Admin" });

app.Run();