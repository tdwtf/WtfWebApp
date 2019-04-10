using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Inedo.Diagnostics;
using Newtonsoft.Json;
using TheDailyWtf.Data;
using TheDailyWtf.Forum;
using TheDailyWtf.Models;
using TheDailyWtf.Security;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    [Authorize]
    [RequireHttps]
    public class AdminController : WtfControllerBase
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            if (!this.User.IsAdmin)
                return Redirect("/admin/my-articles");

            return Redirect("/admin/articles");
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            if (this.User != null)
                return Redirect("/admin");

            return View();
        }

        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            this.Response.Cookies.Add(new HttpCookie("IS_ADMIN", "")
            {
                HttpOnly = false,
                Expires = DateTime.Now.AddDays(-1),
                Path = FormsAuthentication.FormsCookiePath
            });
            this.Response.Cookies.Add(new HttpCookie("tdwtf_token", "")
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(-1),
                Path = FormsAuthentication.FormsCookiePath
            });
            this.Response.Cookies.Add(new HttpCookie("tdwtf_token_name", "")
            {
                HttpOnly = false,
                Expires = DateTime.Now.AddDays(-1),
                Path = FormsAuthentication.FormsCookiePath
            });

            return RedirectToAction("login");
        }

        public ActionResult ReenableSideBar()
        {
            ForumHelper.UnpauseConnections();

            return Redirect("/admin");
        }

        public ActionResult MyArticles()
        {
            return View(new MyArticlesViewModel(this.User.Identity.Name));
        }

        [RequiresAdmin]
        public ActionResult ArticleList()
        {
            return View(new AdminViewModel());
        }

        [RequiresAdmin]
        public ActionResult SeriesList()
        {
            return View(new AdminViewModel());
        }

        [RequiresAdmin]
        public ActionResult LoginList()
        {
            return View(new AdminViewModel());
        }

        [RequiresAdmin]
        public ActionResult FooterAdList()
        {
            return View(new AdminViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            bool validLogin = DB.Authors_ValidateLogin(username, password).Value;

            if (validLogin)
            {
                var author = AuthorModel.GetAuthorBySlug(username);
                var principal = new AuthorPrincipal(author);

                var userData = JsonConvert.SerializeObject(principal.ToSerializableModel());
                var issued = DateTime.Now;
                var expiresDate = issued.AddMinutes(30);
                var authTicket = new FormsAuthenticationTicket(1, author.Slug, issued, expiresDate, false, userData);

                string encTicket = FormsAuthentication.Encrypt(authTicket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
                {
                    HttpOnly = true,
                    Expires = expiresDate,
                    Path = FormsAuthentication.FormsCookiePath
                };
                this.Response.Cookies.Add(cookie);

                var expiresLong = issued.AddYears(2);
                var cookieIsAdmin = new HttpCookie("IS_ADMIN", "1")
                {
                    HttpOnly = false,
                    Expires = expiresLong,
                    Path = FormsAuthentication.FormsCookiePath
                };
                this.Response.Cookies.Add(cookieIsAdmin);

                var ticket = new FormsAuthenticationTicket(1, author.Name, issued, expiresLong, true, "author:" + author.Slug);
                this.Response.SetCookie(new HttpCookie("tdwtf_token", FormsAuthentication.Encrypt(ticket))
                {
                    HttpOnly = true,
                    Expires = expiresLong,
                    Path = FormsAuthentication.FormsCookiePath
                });
                this.Response.SetCookie(new HttpCookie("tdwtf_token_name", author.Name)
                {
                    HttpOnly = false,
                    Expires = expiresLong,
                    Path = FormsAuthentication.FormsCookiePath
                });

                return new RedirectResult(FormsAuthentication.GetRedirectUrl(author.Slug, false));
            }

            return View();
        }

        public ActionResult EditArticle(int? id)
        {
            var model = new EditArticleViewModel(id) { User = this.User };
            if (model.UserCanEdit)
                return View(model);
            else
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
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
            if (!string.IsNullOrEmpty(post.Article.Author.Slug) && !this.User.IsAdmin && post.Article.Author.Slug != this.User.Identity.Name)
                this.ModelState.AddModelError(string.Empty, "Only administrators can change authors.");
            if (!this.ModelState.IsValid)
                return View(post);

            try
            {
                Logger.Information($"Creating or updating article \"{post.Article.Title}\".");
                int? articleId = DB.Articles_CreateOrUpdateArticle(
                    post.Article.Id,
                    post.Article.Slug ?? this.User.Identity.Name,
                    post.PublishedDate,
                    post.Article.Status,
                    post.Article.Author.Slug,
                    post.Article.Title,
                    post.Article.Series.Slug,
                    post.Article.BodyHtml,
                    post.Article.DiscourseTopicId
                );

                post.Article.Id = post.Article.Id ?? articleId;

                return RedirectToRoute(this.User.IsAdmin ? "ArticleListAdmin" : "MyArticlesAdmin");
            }
            catch (Exception ex)
            {
                post.ErrorMessage = ex.ToString();
                return View(post);
            }
        }

        public ActionResult ArticleComments(int id, int page)
        {
            if (this.User == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            var article = ArticleModel.GetArticleById(id);
            if (article == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (this.User.IsAdmin)
                return View(new ArticleCommentsViewModel(article, page, true));
            if (this.User.Identity.Name != article.Author.Slug)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(new ArticleCommentsViewModel(article, page, false));
        }

        public ActionResult CommentModeration(int page)
        {
            if (this.User == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(new HiddenCommentsViewModel(page, this.User.IsAdmin ? null : this.User.Identity.Name));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiresAdmin]
        public ActionResult DeleteComments(DeleteCommentsModel post)
        {
            var commentIdsCsv = string.Join(",", post.Delete);
            Logger.Information($"Deleting comments with IDs \"{commentIdsCsv}\".");

            DB.Comments_DeleteComments(commentIdsCsv);

            return Redirect(Request.UrlReferrer.ToString());
        }

        [RequiresAdmin]
        public ActionResult CommentsByIP(string ip, int page)
        {
            if (this.User == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(UserCommentsViewModel.ByIP(ip, page));
        }

        [RequiresAdmin]
        public ActionResult CommentsByToken(string token, int page)
        {
            if (this.User == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            return View(UserCommentsViewModel.ByToken(token, page));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiresAdmin]
        [ValidateInput(false)]
        public ActionResult EditComment(int article, int comment, string body, string name)
        {
            var articleModel = ArticleModel.GetArticleById(article);
            if (articleModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            var commentModel = CommentModel.GetCommentById(comment);
            if (commentModel == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (body == null)
            {
                body = commentModel.BodyRaw;
                name = commentModel.Username;
                return View(new EditCommentViewModel { Article = articleModel, Comment = commentModel, Post = new CommentFormModel { Body = body, Name = name } });
            }

            DB.Comments_CreateOrUpdateComment(
                Article_Id: article,
                Body_Html: body,
                User_Name: name,
                Posted_Date: commentModel.PublishedDate,
                User_IP: commentModel.UserIP,
                User_Token: commentModel.UserToken,
                Parent_Comment_Id: commentModel.ParentCommentId,
                Comment_Id: comment
            );

            return RedirectToRoute("ArticleCommentsAdmin", new { id = articleModel.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveComment(FeatureCommentViewModel post)
        {
            if (this.User == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (post.Article == null || post.Comment == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var article = ArticleModel.GetArticleById((int)post.Article);
            if (article == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (!this.User.IsAdmin && this.User.Identity.Name != article.Author.Slug)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (!DB.Articles_ApproveComment(article.Id, post.Comment).Value)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (!string.IsNullOrEmpty(Request.QueryString["no-redirect"]))
                return new HttpStatusCodeResult(HttpStatusCode.Accepted);
            return RedirectToRoute("CommentModerationAdmin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FeatureComment(FeatureCommentViewModel post)
        {
            if (this.User == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (post.Article == null || post.Comment == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var article = ArticleModel.GetArticleById((int)post.Article);
            if (article == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (!this.User.IsAdmin && this.User.Identity.Name != article.Author.Slug)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (!DB.Articles_FeatureComment(article.Id, post.Comment).Value)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            return RedirectToRoute("ArticleCommentsAdmin", new { id = article.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnfeatureComment(FeatureCommentViewModel post)
        {
            if (this.User == null)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (post.Article == null || post.Comment == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var article = ArticleModel.GetArticleById((int)post.Article);
            if (article == null)
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            if (!this.User.IsAdmin && this.User.Identity.Name != article.Author.Slug)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            if (!DB.Articles_UnfeatureComment(article.Id, post.Comment).Value)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            return RedirectToRoute("ArticleCommentsAdmin", new { id = article.Id });
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
            DB.Series_CreateOrUpdateSeries(
                post.Series.Slug,
                post.Series.Title,
                post.Series.Description
            );

            return RedirectToRoute("SeriesListAdmin");
        }

        [RequiresAdmin]
        public ActionResult EditAd(int? id)
        {
            return View(new EditAdViewModel(id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequiresAdmin]
        public ActionResult EditAd(EditAdViewModel post)
        {
            DB.Ads_CreateOrUpdateAd(post.Ad.BodyHtml, post.Ad.Id);

            return RedirectToRoute("FooterAdListAdmin");
        }

        [RequiresAdmin]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult DeleteAd(int id)
        {
            DB.Ads_DeleteAd(id);

            return RedirectToRoute("FooterAdListAdmin");
        }

        [RequiresAdmin]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult ReassignAds()
        {
            DB.Articles_FixMissingAds();

            return RedirectToRoute("FooterAdListAdmin");
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
            DB.Authors_CreateOrUpdateAuthor(
                post.Author.Slug,
                post.Author.Name,
                post.Author.IsAdmin,
                post.Author.DescriptionHtml,
                post.Author.ShortDescription,
                Inedo.AH.NullIf(post.Author.ImageUrl, string.Empty),
                post.Author.IsActive
            );

            if (!string.IsNullOrEmpty(post.Password))
            {
                DB.Authors_SetPassword(post.Author.Slug, post.Password);
            }

            return RedirectToRoute("LoginListAdmin");
        }

        public ActionResult ViewAds(DateTime? start, DateTime? end)
        {
            if (!this.User.IsAdmin)
                return RedirectToRoute("MyArticlesAdmin");

            return View(new ViewAdsViewModel(start, end));
        }
    }
}
