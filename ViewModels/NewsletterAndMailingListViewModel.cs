namespace NewsletterWebApp.ViewModels
{
    public class NewsletterAndMailingListViewModel
    {
        public NewsletterViewModel NewsletterModel { get; set; }
        public IEnumerable<MailingListViewModel> MailingLists { get; set; }
    }
}