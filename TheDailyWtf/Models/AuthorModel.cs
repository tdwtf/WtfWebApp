using TheDailyWtf.Data;

namespace TheDailyWtf.Models
{
    public sealed class AuthorModel
    {
        private AuthorModel()
        {
        }

        public string Name { get; set; }
        public string FirstName { get { return this.Name.Split(' ')[0]; } }
        public string ShortDescription { get; set; }
        public string DescriptionHtml { get; set; }
        public string Slug { get; set; }

        public static AuthorModel FromTable(Tables.Authors author)
        {
            return new AuthorModel()
            {
                Name = author.Display_Name,
                ShortDescription = author.ShortBio_Text,
                DescriptionHtml = author.Bio_Html,
                Slug = author.Author_Slug
            };
        }

        public static AuthorModel FromTable(Tables.Articles_Extended article)
        {
            return new AuthorModel()
            {
                Name = article.Author_Display_Name,
                ShortDescription = article.Author_ShortBio_Text,
                DescriptionHtml = article.Author_Bio_Html,
                Slug = article.Author_Slug
            };
        }

        public static AuthorModel GetAuthorBySlug(string slug)
        {
            var author = StoredProcs.Authors_GetAuthorBySlug(slug).Execute();
            return AuthorModel.FromTable(author);
        }
    }
}