using System.Collections.Generic;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class AdminViewModel : WtfViewModelBase
    {
        public IEnumerable<ArticleModel> PublishedArticles { get { return ArticleModel.GetRecentArticles(25); } }
        public IEnumerable<ArticleModel> UnpublishedArticles { get { return ArticleModel.GetUnpublishedArticles(); } }
        public IEnumerable<SeriesModel> AllSeries { get { return SeriesModel.GetAllSeries(); } }
        public IEnumerable<AuthorModel> AllAuthors { get { return AuthorModel.GetAllAuthors(); } }
        public IEnumerable<AdModel> AllAds { get { return AdModel.GetAllFooterAds(); } }
    }
}