using System.Web.Mvc;

namespace TheDailyWtf.Models
{
    public class CommentFormModel
    {
        public int? Parent { get; set; }
        [AllowHtml]
        public string Name { get; set; }
        [AllowHtml]
        public string Body { get; set; }
    }
}