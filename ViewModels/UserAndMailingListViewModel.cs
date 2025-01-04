namespace NewsletterWebApp.ViewModels
{
    public class UserAndMailingListViewModel
    {
        public UserViewModel User { get; set; }
        public IEnumerable<MailingListViewModel> MailingLists { get; set; }
    }
}