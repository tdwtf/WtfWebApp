using System;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Configuration;
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

        public ActionResult Sponsors()
        {
            return View(new HomeIndexViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(ContactFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var writer = new StringWriter();

                using (var smtp = new SmtpClient(WebConfigurationManager.AppSettings["Wtf.Mail.Host"]))
                using (var message = new MailMessage(InedoLib.Util.CoalesceStr(model.ContactForm.Email, WebConfigurationManager.AppSettings["Wtf.Mail.FromAddress"]), WebConfigurationManager.AppSettings["Wtf.Mail.ToAddress"]))
                {
                    writer.WriteLine("To: {0}", model.ContactForm.To);
                    writer.WriteLine("From: {0}", model.ContactForm.Name);
                    writer.WriteLine();
                    writer.WriteLine("Message:");
                    writer.WriteLine("--------------------------------");
                    writer.WriteLine(model.ContactForm.Message);

                    message.Subject = string.Format("[ContactForm] {0}", model.ContactForm.Subject);
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
                var writer = new StringWriter();

                using (var smtp = new SmtpClient(WebConfigurationManager.AppSettings["Wtf.Mail.Host"]))
                using (var message = new MailMessage(InedoLib.Util.CoalesceStr(model.SubmitForm.Email, WebConfigurationManager.AppSettings["Wtf.Mail.FromAddress"]), WebConfigurationManager.AppSettings["Wtf.Mail.ToAddress"]))
                {
                    WriteCommonBody(writer, model.SubmitForm.Name, model.SubmitForm.NameUsage);

                    switch (model.SubmitForm.Type)
                    {
                        case SubmissionType.CodeSod:
                            WriteCodeSodBody(writer, model.SubmitForm.Language, model.SubmitForm.CodeSnippet, model.SubmitForm.Background);
                            AttachFile(message, model.SubmitForm.CodeFile);
                            break;

                        case SubmissionType.Story:
                            WriteStoryBody(writer, model.SubmitForm.TimeFrame, model.SubmitForm.StoryComments);
                            break;

                        case SubmissionType.Errord:
                            WriteErrordBody(writer, model.SubmitForm.ErrordComments);
                            AttachFile(message, model.SubmitForm.ErrordFile);
                            break;

                        default:
                            throw new InvalidOperationException("Invalid submission type");
                    }

                    message.Subject = string.Format("[{0}] - TheDailyWtf Submission", model.SubmitForm.Type);
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
            writer.WriteLine("Name: {0}", submitterName);
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

        private void WriteStoryBody(TextWriter writer, string timeFrame, string comments)
        {
            writer.WriteLine("Time Frame: {0}", timeFrame);
            writer.WriteLine();
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
