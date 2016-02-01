using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Mvc;
using TheDailyWtf.Security;

namespace TheDailyWtf
{
    public abstract class WtfControllerBase : Controller
    {
        protected virtual new AuthorPrincipal User
        {
            get { return base.User as AuthorPrincipal; }
        }

        private class RecaptchaResponse
        {
            [JsonProperty(PropertyName = "success")]
            public bool Success { get; set; }

            [JsonProperty(PropertyName = "error-codes")]
            public IEnumerable<string> ErrorCodes { get; set; }
        }

        protected void CheckRecaptcha()
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

                var response = client.PostAsync("https://www.google.com/recaptcha/api/siteverify", new FormUrlEncodedContent(request)).Result;

                var result = JsonConvert.DeserializeObject<RecaptchaResponse>(response.Content.ReadAsStringAsync().Result);
                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, "The CAPTCHA was invalid. Try again.");
                }
            }
        }
    }
}