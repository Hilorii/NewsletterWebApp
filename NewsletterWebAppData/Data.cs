﻿using Microsoft.EntityFrameworkCore;

namespace NewsletterWebApp.Data
{
    public class User
    {
        public int Id { get; set; } // Klucz główny
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Subscribed { get; set; } = false; // po zaimplementowaniu różnych list subskrypcji można to usunąć
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Admin { get; set; } = false;

        // Relacje
        public ICollection<EmailLogUser> EmailLogUsers { get; set; } // Jeden użytkownik może być przypisany do wielu logów
        public ICollection<Subscription> Subscriptions { get; set; } // Jeden użytkownik może mieć wiele subskrypcji
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
        public string? ImageUrl { get; set; } // być może URL załącznika (dowolnego typu) lub ich lista?
        public bool IsNewsletter { get; set; } = false;
        public int TotalClicks { get; set; } = 0;
        public int TotalOpens { get; set; } = 0;
        public bool IsSent { get; set; } = false;
        public bool IsScheduled { get; set; } = false;

        // Relacje
        public ICollection<EmailLog> EmailLogs { get; set; } = new List<EmailLog>(); // Jeden email może być logowany wiele razy
        public ICollection<EmailMailingList> EmailMailingLists { get; set; } // Jeden email może być przypisany do wielu list mailingowych
    }

    public class MailingList
    {
        public int Id { get; set; } // Klucz główny
        public string Name { get; set; }

        // Relacje
        public ICollection<Subscription> Subscriptions { get; set; } // Jedna lista może mieć wiele subskrypcji
        public ICollection<EmailMailingList> EmailMailingLists { get; set; } // Jedna lista może wieć wiele maili
    }

    public class Subscription
    {
        public int Id { get; set; } // Klucz główny
        public int MailingListId { get; set; }
        public int UserId { get; set; }

        // Relacje
        public MailingList MailingList { get; set; }
        public User User { get; set; }
    }

    public class EmailMailingList
    {
        public int Id { get; set; } // Klucz główny
        public int EmailId { get; set; }
        public int MailingListId { get; set; }

        // Relacje
        public Email Email { get; set; }
        public MailingList MailingList { get; set; }
    }
}
