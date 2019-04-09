using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using TheDailyWtf.Data;
using TheDailyWtf.Security;

namespace TheDailyWtf
{
    public abstract class WtfControllerBase : Controller
    {
        protected virtual new AuthorPrincipal User
        {
            get { return base.User as AuthorPrincipal; }
        }

        protected Tables.Articles_Slim GetRandomArticleInternal()
        {
            if (!(this.HttpContext.Cache["ArticleList"] is Tables.Articles_Slim[] allArticles))
            {
                allArticles = DB.Articles_GetArticlesSlim().ToArray();
                this.HttpContext.Cache.Add("ArticleList", allArticles, null, DateTime.UtcNow.AddHours(1), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            }

            int i = new Random().Next(allArticles.Length);
            return allArticles[i];
        }

        private class RecaptchaResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "error-codes")]
            public IEnumerable<string> ErrorCodes { get; set; }
        }

        protected async Task CheckRecaptchaAsync()
        {
            if (string.IsNullOrEmpty(Request.Form["g-recaptcha-response"]))
            {
                ModelState.AddModelError(string.Empty, "You forgot to check the \"I'm not a robot\" box.");
                return;
            }

            using (var client = new HttpClient())
            {
                var request = new Dictionary<string, string>
                        {
                            { "secret", Config.RecaptchaPrivateKey },
                            { "response", Request.Form["g-recaptcha-response"] },
                            { "remoteip", Request.ServerVariables["REMOTE_ADDR"] },
                        };

                using (var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(request)))
                {
                    var result = JsonConvert.DeserializeObject<RecaptchaResponse>(await response.Content.ReadAsStringAsync());
                    if (!result.Success)
                    {
                        ModelState.AddModelError(string.Empty, "The CAPTCHA was invalid. Try again.");
                    }
                }
            }
        }
    }
}