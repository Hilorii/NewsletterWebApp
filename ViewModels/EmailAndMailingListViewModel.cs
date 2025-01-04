namespace NewsletterWebApp.ViewModels
{
    public class EmailAndMailingListViewModel
    {
        public EmailViewModel Email { get; set; }
        public IEnumerable<MailingListViewModel> MailingLists { get; set; }
    }
}