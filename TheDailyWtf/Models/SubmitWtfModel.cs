using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheDailyWtf.Models
{
    public sealed class SubmitWtfModel
    {
        public SubmissionType Type { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public NameUsage NameUsage { get; set; }

        public string Language { get; set; }
        public string CodeSnippet { get; set; }
        public string Background { get; set; }
        public HttpPostedFile CodeFile { get; set; }

        public string ErrordComments { get; set; }
        public HttpPostedFile ErrordFile { get; set; }

        public string TimeFrame { get; set; }
        public string StoryComments { get; set; }
    }

    public enum NameUsage { Anonymous, FirstNameOnly, FirstNameLastInitial, UseName}
    public enum SubmissionType { CodeSod, Story, Errord }
}