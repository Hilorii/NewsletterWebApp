

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
            
            base.OnModelCreating(modelBuilder);

            // Konfiguracja relacji pomiędzy encjami
            modelBuilder.Entity<EmailLogUser>()
                .HasOne(e => e.EmailLog)
                .WithMany(el => el.EmailLogUsers)
                .HasForeignKey(e => e.EmailLogId);

            modelBuilder.Entity<EmailLogUser>()
                .HasOne(e => e.User)
                .WithMany(u => u.EmailLogUsers)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Click>()
                .HasOne(c => c.EmailLog)
                .WithMany(el => el.Clicks)
                .HasForeignKey(c => c.EmailLogId);

            modelBuilder.Entity<EmailLog>()
                .HasOne(el => el.Email)
                .WithMany(e => e.EmailLogs)
                .HasForeignKey(el => el.EmailId);
            
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Click> Clicks { get; set; }
        public DbSet<EmailLogUser> EmailLogUsers { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailOpen> EmailOpens { get; set; }

    }
}