using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewsletterWebApp.Controllers;
using Quartz;
using NewsletterWebApp.Data;
using NewsletterWebApp.Models;

namespace NewsletterWebApp.Jobs
{
    public class EmailSchedulerJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailSchedulerJob> _logger;

        public EmailSchedulerJob(IServiceProvider serviceProvider, ILogger<EmailSchedulerJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

            var emailsToSend = dbContext.Emails
                .Where(e => e.ScheduledAt <= DateTime.UtcNow && e.IsScheduled)
                .ToList();

            if (!emailsToSend.Any())
            {
                _logger.LogInformation("No emails to send at {Time}", DateTime.UtcNow);
                return;
            }

            foreach (var email in emailsToSend)
            {
                try
                {
                    var users = dbContext.Users
                        .Where(u => !u.Admin && u.Subscribed)
                        .ToList();

                    // Call the existing method to send emails
                    var adminController = scope.ServiceProvider.GetRequiredService<AdminController>();
                    // await adminController.SendEmailsToUsersWithSendGridAsync(email.Title, email.Content, email.Id, users);
                    await adminController.SendEmail(email.Title, email.Content, email.ScheduledAt, true, true);

                    // email.IsScheduled = true;
                    // email.IsSent = false;
                    email.UpdatedAt = DateTime.UtcNow;
                    dbContext.Emails.Update(email);
                    _logger.LogInformation("Email {EmailId} sent successfully.", email.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending email {EmailId}.", email.Id);
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
