

using Microsoft.EntityFrameworkCore;

namespace NewsletterWebApp.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseSerialColumns();
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Click> Clicks { get; set; }
        public DbSet<EmailLogUser> EmailLogUsers { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Email> Emails { get; set; }

    }
}