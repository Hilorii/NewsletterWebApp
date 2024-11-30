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
}