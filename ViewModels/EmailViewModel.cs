namespace NewsletterWebApp.ViewModels
{
    public class EmailViewModel
    {
        public string Title { get; set; }
        public DateTime SentAt { get; set; }
        public int TotalClicks { get; set; }
        public int TotalOpens { get; set; }
    }
}