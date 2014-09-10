using System.Web;
using System.ComponentModel.DataAnnotations;

namespace TheDailyWtf.Models
{
    public class ContactFormModel
    {
        [Required]
        public string To { get; set; }
        [Required]
        public string Subject { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public HttpPostedFileBase File { get; set; }
        [Required]
        public string Message { get; set; }
    }
}