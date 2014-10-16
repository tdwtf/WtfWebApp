using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace TheDailyWtf.Discourse
{
    public sealed class DiscourseApi : IDiscourseApi
    {
        private static readonly object requestLock = new object();

        private string baseUrl;
        private string apiUsername;
        private string apiKey;

        public DiscourseApi(string discourseUrl, string apiUsername, string apiKey)
        {
            this.baseUrl = string.Format("http://{0}", discourseUrl.TrimEnd('/'));
            this.apiUsername = apiUsername;
            this.apiKey = apiKey;
        }

        public Topic CreateTopic(Category category, string title, string body)
        {
            string response = this.PostRequest(
                "/posts",
                new Dictionary<string, string> 
                { 
                    { "category", category.Name },
                    { "title", title },
                    { "raw", body }
                }
            );

            dynamic topic = JsonConvert.DeserializeObject(response);
            return Topic.CreateFromPostJson(topic, title);
        }

        public void SetVisibility(int topicId, bool visible)
        {
            this.PutRequest(
                string.Format("/t/{0}/status", topicId),
                new Dictionary<string, string>()
                {
                    { "status", "visible" },
                    { "enabled", visible.ToString().ToLower() }
                }
            );
        }

        public void SetCategoryTopic(Topic topic, Category category)
        {
            this.SetTopicsCategoryBulk(new[] { topic }, category);
        }

        public void SetTopicsCategoryBulk(IEnumerable<Topic> topics, Category category)
        {
            this.PutRequest(
                "/topics/bulk",
                topics.Select(t => new KeyValuePair<string, string>("topic_ids[]", t.Id.ToString()))
                .Concat(new Dictionary<string, string>()
                {
                    { "operation[type]", "change_category" },
                    { "operation[category_name]", category.Name }
                })
            );
        }

        public IEnumerable<Topic> GetTopicsByCategory(Category category, string filter = "latest")
        {
            var validFilters = new[] { "top", "starred", "unread", "new", "latest" };
            if (!validFilters.Contains(filter))
                throw new ArgumentException("filter");

            string response = this.GetRequest("/category/{0}/l/{1}.json", category.UrlFormatted, filter);
            dynamic json = JsonConvert.DeserializeObject(response);
            dynamic topics = json.topic_list.topics;
            foreach (dynamic topic in topics)
                yield return Topic.CreateFromJson(topic);
        }

        public IEnumerable<Topic> GetTopics(string filter = "latest")
        {
            var validFilters = new[] { "top", "starred", "unread", "new", "latest" };
            if (!validFilters.Contains(filter))
                throw new ArgumentException("filter");

            string response = this.GetRequest("/{0}.json", filter);
            dynamic json = JsonConvert.DeserializeObject(response);
            dynamic topics = json.topic_list.topics;
            foreach (dynamic topic in topics)
                yield return Topic.CreateFromJson(topic);
        }

        public Topic GetTopic(int id)
        {
            string response = this.GetRequest("/t/{0}.json", id);
            dynamic topic = JsonConvert.DeserializeObject(response);

            return Topic.CreateFromJson(topic);
        }

        public Post GetReplyPost(int postId)
        {
            string response = this.GetRequest("/posts/{0}.json", postId);
            dynamic post = JsonConvert.DeserializeObject(response);

            return Post.CreateFromJson(post);
        }

        public void DeletePost(int postId)
        {
            this.DeleteRequest("/posts/{0}", postId);
        }

        public string CreateUser(string name, string username, string email, string password)
        {
            string response = this.PostRequest(
                "/users",
                new Dictionary<string, string>()
                {
                    { "name", name },
                    { "username", username },
                    { "email", email },
                    { "password", password }
                }
            );

            return response;
        }

        private string GetRequestUrl(string relativeUrl)
        {
            return string.Format(
                "{0}{1}{4}api_key={2}&api_username={3}",
                this.baseUrl,
                relativeUrl,
                Uri.EscapeDataString(this.apiKey),
                Uri.EscapeDataString(this.apiUsername),
                relativeUrl.Contains("?") ? "&" : "?");
        }

        private string GetRequest(string urlFormat, params object[] args)
        {
            return this.GetOrDeleteRequest("GET", urlFormat, args);
        }

        private string DeleteRequest(string urlFormat, params object[] args)
        {
            return this.GetOrDeleteRequest("DELETE", urlFormat, args);
        }

        private string GetOrDeleteRequest(string method, string urlFormat, params object[] args)
        {
            lock (requestLock)
            {
                string relativeUrl = string.Format(urlFormat, args);

                string requestUrl = this.GetRequestUrl(relativeUrl);

                var request = WebRequest.Create(requestUrl);
                request.Method = method;
                request.Timeout = Config.Discourse.ApiRequestTimeout;
                try
                {
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        return new StreamReader(stream).ReadToEnd();
                    }
                }
                catch (TimeoutException tex)
                {
                    DiscourseHelper.PauseDiscourseConnections(tex, 10);
                    throw;
                }
                catch (WebException wex)
                {
                    DiscourseHelper.PauseDiscourseConnections(wex, 10);
                    throw ParseFirstError(wex);
                }
            }
        }

        private string PostRequest(string relativeUrl, IEnumerable<KeyValuePair<string, string>> postData)
        {
            return this.PutOrPostRequest(relativeUrl, postData, "POST");
        }

        private string PutRequest(string relativeUrl, IEnumerable<KeyValuePair<string, string>> putData)
        {
            return this.PutOrPostRequest(relativeUrl, putData, "PUT");
        }

        private string PutOrPostRequest(string relativeUrl, IEnumerable<KeyValuePair<string, string>> postData, string method)
        {
            lock (requestLock)
            {
                string requestUrl = this.GetRequestUrl(relativeUrl);

                var request = WebRequest.Create(requestUrl);
                request.Method = method;
                request.Timeout = Config.Discourse.ApiRequestTimeout;
                request.ContentType = "application/x-www-form-urlencoded";
                try
                {
                    using (var body = request.GetRequestStream())
                    using (var writer = new StreamWriter(body))
                    {
                        foreach (var pair in postData)
                        {
                            writer.Write("&{0}={1}", HttpUtility.UrlEncode(pair.Key), HttpUtility.UrlEncode(pair.Value));
                        }
                    }

                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        return new StreamReader(stream).ReadToEnd();
                    }
                }
                catch (TimeoutException tex)
                {
                    DiscourseHelper.PauseDiscourseConnections(tex, 10);
                    throw;
                }
                catch (WebException wex)
                {
                    DiscourseHelper.PauseDiscourseConnections(wex, 10);
                    throw ParseFirstError(wex);
                }
            }
        }

        private static Exception ParseFirstError(WebException wex)
        {
            try
            {
                string jsonErrors = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                wex.Response.Dispose();
                dynamic json = JsonConvert.DeserializeObject(jsonErrors);
                if (json == null)
                    return wex;

                return new InvalidOperationException(json.errors[0].ToString(), wex);
            }
            catch
            {
                var ex = new InvalidOperationException("Unknown error connecting to the forum API. Ensure the host name and API key settings are correct in web.config. The comment and sidebar categories must also exist on Discourse.", wex);
                DiscourseHelper.PauseDiscourseConnections(ex, 10);
                return ex;
            }
        }
    }
}