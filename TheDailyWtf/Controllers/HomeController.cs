using System;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Inedo;
using TheDailyWtf.Models;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class HomeController : WtfControllerBase
    {
        //
        // GET: /Home/

        [OutputCache(CacheProfile = CacheProfile.Timed1Minute)]
        public ActionResult Index()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Search()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Contact()
        {
            return View(new ContactFormViewModel());
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult Sponsors()
        {
            return View(new HomeIndexViewModel());
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes, VaryByParam = "*")]
        public ActionResult Rss()
        {
            if (Request.QueryString["fbsrc"] != "Y" && Request.QueryString["sneak"] != "Y")
                return new RedirectResult("http://syndication.thedailywtf.com/TheDailyWtf", false);

            return new RssArticlesResult(ArticleModel.GetRecentArticles(15));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                using(var writer = new StringWriter())
                using (var smtp = new SmtpClient(Config.Wtf.Mail.Host))
                using (var message = new MailMessage(InedoLib.Util.CoalesceStr(model.ContactForm.Email, Config.Wtf.Mail.FromAddress), Config.Wtf.Mail.ToAddress))
                {
                    writer.WriteLine("To: {0}", model.ContactForm.To);
                    writer.WriteLine("Your Name: {0}", model.ContactForm.Name);
                    writer.WriteLine();
                    writer.WriteLine("Message:");
                    writer.WriteLine("--------------------------------");
                    writer.WriteLine(model.ContactForm.Message);

                    message.Subject = model.ContactForm.Subject.Substring(0, Math.Min(model.ContactForm.Subject.Length, 100));
                    message.Body = writer.ToString();
                    AttachFile(message, model.ContactForm.File);

                    smtp.Send(message);
                }

                return View(new ContactFormViewModel { SuccessMessage = "Your feedback has been submitted, thank you!" });
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        public ActionResult StorySubmissionGuidelines()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Submit()
        {
            return View(new SubmitWtfViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(SubmitWtfViewModel model)
        {
            if (!this.ModelState.IsValid)
                return View(model);

            try
            {
                using(var writer = new StringWriter())
                using (var smtp = new SmtpClient(Config.Wtf.Mail.Host))
                using (var message = new MailMessage(InedoLib.Util.CoalesceStr(model.SubmitForm.Email, Config.Wtf.Mail.FromAddress), Config.Wtf.Mail.ToAddress))
                {
                    WriteCommonBody(writer, model.SubmitForm.Name, model.SubmitForm.NameUsage);

                    switch (model.SubmitForm.Type)
                    {
                        case SubmissionType.CodeSod:
                            message.Subject = "[Code]";
                            WriteCodeSodBody(writer, model.SubmitForm.Language, model.SubmitForm.CodeSnippet, model.SubmitForm.Background);
                            AttachFile(message, model.SubmitForm.CodeFile);
                            break;

                        case SubmissionType.Story:
                            message.Subject = "[Story]";
                            WriteStoryBody(writer, model.SubmitForm.StoryComments);
                            break;

                        case SubmissionType.Errord:
                            message.Subject = "[Error'd]";
                            WriteErrordBody(writer, model.SubmitForm.ErrordComments);
                            AttachFile(message, model.SubmitForm.ErrordFile);
                            break;

                        default:
                            throw new InvalidOperationException("Invalid submission type");
                    }

                    message.Subject = message.Subject + " " + model.SubmitForm.Title;
                    message.Body = writer.ToString();

                    smtp.Send(message);
                }

                return View(new SubmitWtfViewModel { ShowLeaderboardAd = false, SuccessMessage = "Your submission was sent, thank you!" });
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        private void AttachFile(MailMessage message, HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength < 1)
                return;

            message.Attachments.Add(new Attachment(file.InputStream, file.FileName));
        }

        private void WriteCommonBody(TextWriter writer, string submitterName, NameUsage nameUsage)
        {
            writer.WriteLine("Your Name: {0}", submitterName);
            writer.WriteLine(
                "Name Usage: {0}",
                nameUsage == NameUsage.FullName ? "You may use the full name" :
                nameUsage == NameUsage.FirstNameLastInitial ? "First Name, Last Initial" :
                nameUsage == NameUsage.FirstNameOnly ? "First Name Only" :
                "ANONYMOUS"
            );
        }

        private void WriteCodeSodBody(TextWriter writer, string language, string codeSnippet, string background)
        {
            writer.WriteLine("Language: {0}", language);
            writer.WriteLine();
            writer.WriteLine("Background:");
            writer.WriteLine("--------------------------------");
            writer.WriteLine(background);
            writer.WriteLine();
            writer.WriteLine("Code Snippet:");
            writer.WriteLine("--------------------------------");
            writer.WriteLine(codeSnippet);
        }

        private void WriteStoryBody(TextWriter writer, string comments)
        {
            writer.WriteLine("Comments:");
            writer.WriteLine("--------------------------------");
            writer.WriteLine(comments);
        }

        private void WriteErrordBody(TextWriter writer, string comments)
        {
            writer.WriteLine();
            writer.WriteLine("Comments:");
            writer.WriteLine("--------------------------------");
            writer.WriteLine(comments);
        }
    }
}
