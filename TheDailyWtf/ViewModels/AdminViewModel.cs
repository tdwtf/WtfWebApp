using System.Collections.Generic;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class AdminViewModel : WtfViewModelBase
    {
        public IEnumerable<ArticleModel> UnpublishedArticles { get { return ArticleModel.GetUnpublishedArticles(); } }
    }
}