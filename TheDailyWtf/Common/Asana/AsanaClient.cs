using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace TheDailyWtf.Common.Asana
{
    public sealed class AsanaClient
    {
        public static AsanaClient Instance { get; } = new AsanaClient();
        public const long ProjectId = 614965995036490;
        public const long CodeSodTagId = 3711459423565;
        public const long StoryTagId = 614974001148585;
        public const long ErrordTagId = 614974001148586;

        private readonly HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://app.asana.com/api/1.0/"),
            DefaultRequestHeaders =
            {
                { "Authorization", "Bearer " + Config.Asana.AccessToken }
            }
        };

        private AsanaClient()
        {
        }

        public async Task CreateTaskAsync(long tagID, string name, string notes, params HttpPostedFileBase[] attachments)
        {
            long id;
            using (var response = await this.client.PostAsync("task", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "project", ProjectId.ToString() },
                { "name", name },
                { "notes", notes },
                { "tags", tagID.ToString() }
            })))
            {
                response.EnsureSuccessStatusCode();

                var content = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync(), new { data = new { id = 0L } });
                id = content.data.id;
            }

            foreach (var attachment in attachments)
            {
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(attachment.InputStream)
                    {
                        Headers =
                        {
                            ContentType = MediaTypeHeaderValue.Parse(attachment.ContentType),
                            ContentLength = attachment.ContentLength
                        }
                    }, "file", attachment.FileName);

                    using (var response = await this.client.PostAsync($"task/{id}/attachments", content))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}