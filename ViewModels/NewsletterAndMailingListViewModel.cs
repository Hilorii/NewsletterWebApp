﻿namespace NewsletterWebApp.ViewModels
{
    public class NewsletterAndMailingListViewModel
    {
        public NewsletterViewModel Newsletter { get; set; }
        public IEnumerable<MailingListViewModel> MailingLists { get; set; }
    }
}