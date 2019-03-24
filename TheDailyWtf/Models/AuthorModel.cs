using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheDailyWtf.Data;

namespace TheDailyWtf.Models
{
    public sealed class AuthorModel
    {
        public static readonly string DefaultImageUrl = "/content/images/authors/no-image.png";

        private string imageUrl;

        public string Name { get; set; }
        public string FirstName { get { return this.Name.Split(' ')[0]; } }
        public string ShortDescription { get; set; }
        [AllowHtml]
        public string DescriptionHtml { get; set; }
        public string Slug { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public string ImageUrl 
        { 
            get { return string.IsNullOrEmpty(imageUrl) ? AuthorModel.DefaultImageUrl : this.imageUrl; } 
            set { this.imageUrl = value; } 
        }

        public static AuthorModel FromTable(Tables.Authors author)
        {
            return new AuthorModel()
            {
                Name = author.Display_Name,
                ShortDescription = author.ShortBio_Text,
                DescriptionHtml = author.Bio_Html,
                Slug = author.Author_Slug,
                IsAdmin = author.Admin_Indicator,
                ImageUrl = author.Image_Url,
                IsActive = author.Active_Indicator
            };
        }

        public static AuthorModel FromTable(Tables.Articles_Extended article)
        {
            return new AuthorModel()
            {
                Name = article.Author_Display_Name,
                ShortDescription = article.Author_ShortBio_Text,
                DescriptionHtml = article.Author_Bio_Html,
                Slug = article.Author_Slug,
                IsAdmin = article.Author_Admin_Indicator,
                ImageUrl = article.Author_Image_Url,
                IsActive = article.Author_Active_Indicator
            };
        }

        public static AuthorModel GetAuthorBySlug(string slug)
        {
            var author = DB.Authors_GetAuthorBySlug(slug);
            if (author == null)
            {
                return null;
            }
            return AuthorModel.FromTable(author);
        }

        public static IEnumerable<AuthorModel> GetAllAuthors()
        {
            var authors = DB.Authors_GetAuthors();
            return authors.Select(a => AuthorModel.FromTable(a));
        }

        public static IEnumerable<AuthorModel> GetActiveAuthors()
        {
            return GetAllAuthors().Where(a => a.IsActive);
        }
    }
}