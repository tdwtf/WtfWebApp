using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace TheDailyWtf.Models
{
    public sealed class SubmitWtfModel
    {
        [Required]
        public SubmissionType Type { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public NameUsage NameUsage { get; set; }

        public string Language { get; set; }
        [AllowHtml]
        public string CodeSnippet { get; set; }
        [AllowHtml]
        public string Background { get; set; }
        public HttpPostedFileBase CodeFile { get; set; }

        [AllowHtml]
        public string ErrordComments { get; set; }
        public HttpPostedFileBase ErrordFile { get; set; }

        public string TimeFrame { get; set; }
        [AllowHtml]
        public string StoryComments { get; set; }
    }

    public enum NameUsage { Anonymous, FirstNameOnly, FirstNameLastInitial, FullName }
    public enum SubmissionType { CodeSod, Story, Errord }
}