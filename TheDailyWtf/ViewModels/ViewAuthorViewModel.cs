using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewAuthorViewModel : WtfViewModelBase
    {
        public ViewAuthorViewModel(string slug)
        {
            this.Author = AuthorModel.GetAuthorBySlug(slug);
        }

        public AuthorModel Author { get; set; }
        public IEnumerable<ArticleItemViewModel> Articles
        {
            get
            {
                return ArticleModel.GetRecentArticlesByAuthor(this.Author.Slug)
                    .Select(a => new ArticleItemViewModel(a) { DisplayAuthorLink = false });
            }
        }
    }
}