

namespace NewsletterWebApp.Data
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Subscribed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}