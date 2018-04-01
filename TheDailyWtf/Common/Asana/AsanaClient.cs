using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TheDailyWtf.Common.Asana
{
    public sealed class AsanaClient
    {
        public static AsanaClient Instance { get; } = new AsanaClient();
        public const long ProjectId = 614965995036490;
        public const long CodeSodTagId = 3711459423565;
        public const long StoryTagId = 614974001148585;
        public const long ErrordTagId = 614974001148586;

        private HttpClient CreateClient()
        {
            return new HttpClient()
            {
                BaseAddress = new Uri("https://app.asana.com/api/1.0/"),
                DefaultRequestHeaders =
                {
                    { "Authorization", "Bearer " + Config.Asana.AccessToken }
                }
            };
        }

        private AsanaClient()
        {
            ServicePointManager.SecurityProtocol = ServicePointManager.SecurityProtocol | SecurityProtocolType.Tls12;
        }

        public async Task CreateTaskAsync(long tagID, string name, string notes, params KeyValuePair<string, HttpContent>[] attachments)
        {
            using (var client = this.CreateClient())
            {
                long id;
                using (var response = await client.PostAsync("tasks", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "projects", ProjectId.ToString() },
                    { "name", name },
                    { "notes", notes },
                    { "tags", tagID.ToString() }
                })).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var content = JsonConvert.DeserializeAnonymousType(await response.Content.ReadAsStringAsync().ConfigureAwait(false), new { data = new { id = 0L } });
                    id = content.data.id;
                }

                foreach (var attachment in attachments)
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(attachment.Value, "file", attachment.Key);

                        using (var response = await client.PostAsync($"tasks/{id}/attachments", content).ConfigureAwait(false))
                        {
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
        }
    }
}