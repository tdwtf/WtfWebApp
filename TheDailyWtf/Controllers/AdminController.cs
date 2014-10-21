using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Inedo.Data;
using Newtonsoft.Json;
using TheDailyWtf.Data;
using TheDailyWtf.Discourse;
using TheDailyWtf.Models;
using TheDailyWtf.Security;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    [Authorize]
    public class AdminController : WtfControllerBase
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            if (!this.User.IsAdmin)
                return RedirectToAction("MyArticles");

            return View(new AdminViewModel());
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (this.User != null)
                return new RedirectResult("/admin");

            return View();
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            
            return RedirectToAction("login");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            bool validLogin = StoredProcs.Authors_ValidateLogin(username, password).Execute().Value;

            if (validLogin)
            {
                var author = AuthorModel.GetAuthorBySlug(username);
                var principal = new AuthorPrincipal(author);

                var userData = JsonConvert.SerializeObject(principal.ToSerializableModel());
                var expiresDate = DateTime.Now.AddMinutes(30);
                var authTicket = new FormsAuthenticationTicket(1, author.Slug, DateTime.Now, expiresDate, false, userData);

                string encTicket = FormsAuthentication.Encrypt(authTicket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
                {
                    HttpOnly = true,
                    Expires = expiresDate,
                    Path = FormsAuthentication.FormsCookiePath
                };
                this.Response.Cookies.Add(cookie);

                return new RedirectResult(FormsAuthentication.GetRedirectUrl(author.Slug, false));
            }

            return View();
        }

        public ActionResult EditArticle(int? id)
        {
            return View(new EditArticleViewModel(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult EditArticle(EditArticleViewModel post)
        {
            if (string.IsNullOrEmpty(post.Article.Series.Slug))
                this.ModelState.AddModelError(string.Empty, "A series is required");
            if (string.IsNullOrEmpty(post.Article.Author.Slug))
                this.ModelState.AddModelError(string.Empty, "An author is required");
            if (!this.ModelState.IsValid)
                return View(post);

            try
            {
                if (post.CreateCommentDiscussionChecked)
                    DiscourseHelper.CreateCommentDiscussion(post.Article);
                if (post.OpenCommentDiscussionChecked && post.Article.DiscourseTopicId > 0)
                    DiscourseHelper.OpenCommentDiscussion((int)post.Article.Id, (int)post.Article.DiscourseTopicId);

                StoredProcs.Articles_CreateOrUpdateArticle(
                    post.Article.Id,
                    post.Article.Slug,
                    post.PublishedDate,
                    post.Article.Status,
                    post.Article.Author.Slug,
                    post.Article.Title,
                    post.Article.Series.Slug,
                    post.Article.BodyHtml,
                    post.Article.DiscourseTopicId
                  ).Execute();

                return RedirectToAction("index");
            }
            catch (Exception ex)
            {
                post.ErrorMessage = ex.ToString();
                return View(post);
            }
        }

        [RequiresAdmin]
        public ActionResult EditSeries(string slug)
        {
            return View(new EditSeriesViewModel(slug));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiresAdmin]
        public ActionResult EditSeries(EditSeriesViewModel post)
        {
            StoredProcs.Series_CreateOrUpdateSeries(
                post.Series.Slug, 
                post.Series.Title, 
                post.Series.Description
              ).Execute();

            return RedirectToAction("index");
        }

        public ActionResult EditAuthor(string slug)
        {
            return View(new EditAuthorViewModel(slug));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiresAdmin]
        public ActionResult EditAuthor(EditAuthorViewModel post)
        {
            StoredProcs.Authors_CreateOrUpdateAuthor(
                post.Author.Slug,
                post.Author.Name,
                post.Author.IsAdmin,
                post.Author.DescriptionHtml,
                post.Author.ShortDescription,
                Inedo.InedoLib.Util.NullIf(post.Author.ImageUrl, string.Empty)
              ).Execute();

            if (!string.IsNullOrEmpty(post.Password))
            {
                StoredProcs.Authors_SetPassword(post.Author.Slug, post.Password).Execute();
            }

            return RedirectToAction("index");
        }
    }
}