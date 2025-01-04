using NewsletterWebApp.Data;

namespace NewsletterWebApp.ViewModels
{
    public class NewsletterViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<int> MailingListIds { get; set; } = new List<int>();
    }
}