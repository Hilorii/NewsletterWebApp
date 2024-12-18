namespace NewsletterWebApp.Data
{
    public class User
    {
        public int Id { get; set; } // Klucz główny
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Subscribed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Admin { get; set; } = false;

        // Relacje
        public ICollection<EmailLogUser> EmailLogUsers { get; set; } // Jeden użytkownik może być przypisany do wielu logów
    }

    public class Click
    {
        public int Id { get; set; } // Klucz główny
        public int EmailLogId { get; set; } // Klucze obce
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacje
        public EmailLog EmailLog { get; set; }
    }
    public class EmailOpen
    {
        public int Id { get; set; } // Klucz główny
        public int EmailLogId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacje
        public EmailLog EmailLog { get; set; }
    }

    public class EmailLogUser
    {
        public int Id { get; set; } // Klucz główny
        public int EmailLogId { get; set; }
        public int UserId { get; set; }

        // Relacje
        public EmailLog EmailLog { get; set; }
        public User User { get; set; }
    }

    public class EmailLog
    {
        public int Id { get; set; } // Klucz główny
        public int EmailId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Relacje
        public Email Email { get; set; }
        public ICollection<Click> Clicks { get; set; } // Jeden log może mieć wiele kliknięć
        public ICollection<EmailLogUser> EmailLogUsers { get; set; } // Jeden log może być przypisany do wielu użytkowników
    }

public class Email
{
    public int Id { get; set; } // Klucz główny
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ScheduledAt { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsNewsletter { get; set; } = false;
    public int TotalClicks { get; set; } = 0;
    public int TotalOpens { get; set; } = 0;
    public bool IsSent { get; set; }

    // Relacje
    public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>();// Jeden email może być logowany wiele razy
}
}
