namespace NewsletterWebApp.ViewModels
{
    public class UserViewModel
    {
        public string Email { get; set; }
        public bool IsSubscribed { get; set; } // ta składowa powoli odchodzi do lamusa
        public List<int> MailingListSubscriptionIds { get; set; }
        public bool IsAdmin { get; set; }
    }
}