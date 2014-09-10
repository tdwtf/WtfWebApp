using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class SubmitWtfViewModel : HomeIndexViewModel
    {
        public SubmitWtfViewModel()
        {
            this.ShowLeaderboardAd = false;
            this.SubmitForm = new SubmitWtfModel();
        }

        public SubmitWtfModel SubmitForm { get; set; }
    }
}