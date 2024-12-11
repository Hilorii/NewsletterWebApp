namespace NewsletterWebApp.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Subscribed { get; set; } =  false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Admin { get; set; } = false; 
    }
    public class Click
    {
        public int Id { get; set; }
        public int EmailLogId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
    public class EmailLogUser
    {
        public int Id { get; set; }
        public int EmailLogId { get; set; }
        public int UserId { get; set; }
    }
    public class EmailLog
    {
        public int Id { get; set; }
        public int EmailId { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
    public class Email
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string ImageUrl { get; set; }
        public bool IsNewsletter { get; set; } = false;
        
    }
}