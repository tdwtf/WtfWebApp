using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class EditAuthorViewModel : WtfViewModelBase
    {
        public EditAuthorViewModel()
        {
            this.Author = new AuthorModel();
        }

        public EditAuthorViewModel(string slug)
        {
            if (slug != null)
                this.Author = AuthorModel.GetAuthorBySlug(slug);
            else
                this.Author = new AuthorModel();
        }

        public string Password { get; set; }
        public string CustomImageUrl { get { return this.Author.ImageUrl == AuthorModel.DefaultImageUrl ? "" : this.Author.ImageUrl; } }
        public AuthorModel Author { get; set; }
        public string Heading { get { return this.Author.Slug != null ? string.Format("Edit Author {0}", this.Author.Name) : "Create New Author"; } }
    }
}