using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ContactFormViewModel : HomeIndexViewModel
    {
        public ContactFormViewModel()
        {
            this.ShowLeaderboardAd = false;
            this.ContactForm = new ContactFormModel();
        }

        public ContactFormModel ContactForm { get; set; }
    }
}