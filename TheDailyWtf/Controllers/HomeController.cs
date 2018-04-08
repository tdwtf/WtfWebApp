using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Inedo;
using TheDailyWtf.Common.Asana;
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

        // TODO: this is just the front page
        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult Search()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Contact()
        {
            return View(new ContactFormViewModel() { SelectedContact = HttpUtility.UrlDecode(this.Request.QueryString.ToString()) });
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult Sponsors()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Rss()
        {
            if (Request.QueryString["fbsrc"] != "Y" && Request.QueryString["sneak"] != "Y")
                return new RedirectResult("http://syndication.thedailywtf.com/TheDailyWtf", false);

            return new RssArticlesResult(ArticleModel.GetRecentArticles(15));
        }

        private async Task SendMailAsync(MailMessage message)
        {
            using (var smtp = new SmtpClient(Config.Wtf.Mail.Host, Config.Wtf.Mail.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(Config.Wtf.Mail.Username, Config.Wtf.Mail.Password)
            })
            {
                await smtp.SendMailAsync(message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(ContactFormViewModel model)
        {
            if (this.ModelState.IsValid)
                await this.CheckRecaptchaAsync();

            if (!this.ModelState.IsValid)
                return View(model);

            try
            {
                using (var writer = new StringWriter())
                using (var message = new MailMessage(new MailAddress(Config.Wtf.Mail.FromAddress, model.ContactForm.Name), new MailAddress(Config.Wtf.Mail.ToAddress)))
                {
                    message.ReplyToList.Add(model.ContactForm.Email);

                    if (Config.Wtf.Mail.CustomEmailAddresses.TryGetValue(model.ContactForm.To, out var customToAddress))
                    {
                        message.To.Clear();
                        message.To.Add(customToAddress);
                    }

                    writer.WriteLine("To: {0}", model.ContactForm.To);
                    writer.WriteLine("Your Name: {0}", model.ContactForm.Name);
                    writer.WriteLine("From: {0}", model.ContactForm.Email);
                    writer.WriteLine();
                    writer.WriteLine("Message:");
                    writer.WriteLine("--------------------------------");
                    writer.WriteLine(model.ContactForm.Message);

                    message.Subject = model.ContactForm.Subject.Substring(0, Math.Min(model.ContactForm.Subject.Length, 100));
                    message.Body = writer.ToString();
                    AttachFile(message, model.ContactForm.File);

                    await this.SendMailAsync(message);
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
        public async Task<ActionResult> Submit(SubmitWtfViewModel model)
        {
            if (this.ModelState.IsValid)
                await this.CheckRecaptchaAsync();

            if (!this.ModelState.IsValid)
                return View(model);

            try
            {
                using (var writer = new StringWriter())
                using (var message = new MailMessage(new MailAddress(Config.Wtf.Mail.FromAddress, model.SubmitForm.Name), new MailAddress(Config.Wtf.Mail.ToAddress)))
                {
                    message.ReplyToList.Add(model.SubmitForm.Email);

                    WriteCommonBody(writer, model.SubmitForm.Name, model.SubmitForm.NameUsage, model.SubmitForm.Email);

                    long tag;
                    string title;
                    var attachments = new KeyValuePair<string, HttpContent>[0];
                    switch (model.SubmitForm.Type)
                    {
                        case SubmissionType.CodeSod:
                            tag = AsanaClient.CodeSodTagId;
                            title = "[CodeSOD] ";
                            WriteCodeSodBody(writer, model.SubmitForm.Language, model.SubmitForm.CodeSnippet, model.SubmitForm.Background);
                            attachments = AttachFile(message, model.SubmitForm.CodeFile, true);
                            break;

                        case SubmissionType.Story:
                            tag = AsanaClient.StoryTagId;
                            title = "[Story] ";
                            WriteStoryBody(writer, model.SubmitForm.StoryComments);
                            break;

                        case SubmissionType.Errord:
                            tag = AsanaClient.ErrordTagId;
                            title = "[Error'd] ";
                            WriteErrordBody(writer, model.SubmitForm.ErrordComments);
                            attachments = AttachFile(message, model.SubmitForm.ErrordFile, true);
                            break;

                        default:
                            throw new InvalidOperationException("Invalid submission type");
                    }

                    title += model.SubmitForm.Title;
                    message.Subject = title;
                    message.Body = writer.ToString();

                    await Task.WhenAll(
                        AsanaClient.Instance.CreateTaskAsync(tag, title, writer.ToString(), attachments),
                        this.SendMailAsync(message)
                    );
                }

                return View(new SubmitWtfViewModel { ShowLeaderboardAd = false, SuccessMessage = "Your submission was sent, thank you!" });
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                return View(model);
            }
        }

        private KeyValuePair<string, HttpContent>[] AttachFile(MailMessage message, HttpPostedFileBase file, bool returnContent = false)
        {
            if (file == null || file.ContentLength < 1)
                return new KeyValuePair<string, HttpContent>[0];

            var memory = new MemoryStream();
            file.InputStream.CopyTo(memory);

            message.Attachments.Add(new Attachment(memory, file.FileName, file.ContentType));
            if (!returnContent)
            {
                return new KeyValuePair<string, HttpContent>[0];
            }

            return new[]
            {
                new KeyValuePair<string, HttpContent>(file.FileName, new ByteArrayContent(memory.ToArray())
                {
                    Headers =
                    {
                        ContentLength = file.ContentLength,
                        ContentType = MediaTypeHeaderValue.Parse(file.ContentType)
                    }
                })
            };
        }

        private void WriteCommonBody(TextWriter writer, string submitterName, NameUsage nameUsage, string email)
        {
            writer.WriteLine("Your Name: {0}", submitterName);
            writer.WriteLine(
                "Name Usage: {0}",
                nameUsage == NameUsage.FullName ? "You may use the full name" :
                nameUsage == NameUsage.FirstNameLastInitial ? "First Name, Last Initial" :
                nameUsage == NameUsage.FirstNameOnly ? "First Name Only" :
                "ANONYMOUS"
            );
            writer.WriteLine("Email (do not publish): {0}", email);
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
