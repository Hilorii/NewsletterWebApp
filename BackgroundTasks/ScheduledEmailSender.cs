using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NewsletterWebApp.Controllers;
using NewsletterWebApp.Data;

public class ScheduledEmailSender : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledEmailSender> _logger;

    public ScheduledEmailSender(IServiceProvider serviceProvider, ILogger<ScheduledEmailSender> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                var adminController = scope.ServiceProvider.GetRequiredService<AdminController>();

                // Wywołanie metody do wysyłania zaplanowanych e-maili
                await adminController.SendScheduledEmails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas wysyłania zaplanowanych e-maili.");
            }

            // Czekaj 1 minutę przed kolejną iteracją
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}