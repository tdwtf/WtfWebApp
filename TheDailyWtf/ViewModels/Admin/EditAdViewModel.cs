using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class EditAdViewModel : WtfViewModelBase
    {
        public EditAdViewModel()
        {
            this.Ad = new AdModel();
        }

        public EditAdViewModel(int? id)
        {
            this.AdId = id;
            if (id != null)
                this.Ad = AdModel.GetFooterAdById((int)id);
            else
                this.Ad = new AdModel();
        }

        public int? AdId { get; set; }
        public AdModel Ad { get; set; }
        public string Heading { get { return this.AdId != null ? "Edit Footer Ad" : "Create New Footer Ad"; } }
    }
}