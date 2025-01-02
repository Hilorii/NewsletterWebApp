using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

            // Relationships configuration
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

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.MailingList)
                .WithMany(l => l.Subscriptions)
                .HasForeignKey(s => s.MailingListId);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.User)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.UserId);

            // Apply DateTime as UTC for all entities
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

                foreach (var property in properties)
                {
                    var isNullable = property.PropertyType == typeof(DateTime?);
                    var converter = new ValueConverter<DateTime, DateTime>(
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc), // To database
                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)); // From database

                    modelBuilder.Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(converter)
                        .IsRequired(!isNullable); // If not nullable, set as required
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Click> Clicks { get; set; }
        public DbSet<EmailLogUser> EmailLogUsers { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailOpen> EmailOpens { get; set; }
        public DbSet<MailingList> MailingLists { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
    }
}
