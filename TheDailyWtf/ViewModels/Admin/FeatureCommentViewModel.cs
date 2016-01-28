using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class FeatureCommentViewModel : WtfViewModelBase
    {
        public FeatureCommentViewModel()
        {
        }

        public int? Article { get; set; }
        public int? Comment { get; set; }
    }
}