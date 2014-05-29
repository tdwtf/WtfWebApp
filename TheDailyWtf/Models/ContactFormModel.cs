using System.Web;

namespace TheDailyWtf.Models
{
    public class ContactFormModel
    {
        public string To { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public HttpPostedFileBase File { get; set; }
        public string Message { get; set; }
    }
}