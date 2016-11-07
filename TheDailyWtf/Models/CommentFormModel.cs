using System.Web.Mvc;

namespace TheDailyWtf.Models
{
    public class CommentFormModel
    {
        public const int MaxBodyLength = 2048;

        public int? Parent { get; set; }
        [AllowHtml]
        public string Name { get; set; }
        [AllowHtml]
        public string Body { get; set; }
    }
}