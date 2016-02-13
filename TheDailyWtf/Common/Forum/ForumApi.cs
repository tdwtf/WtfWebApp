using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using Inedo.Diagnostics;
using Newtonsoft.Json;

namespace TheDailyWtf.Forum
{
    public sealed class ForumApi : IForumApi
    {
        private static readonly object requestLock = new object();

        private string baseUrl;

        public ForumApi(string forumUrl)
        {
            this.baseUrl = string.Format("https://{0}", forumUrl.TrimEnd('/'));
        }

        public IEnumerable<Topic> GetTopicsByCategory(Category category)
        {
            string response = this.GetRequest("/api/category/{0}", category.UrlFormatted);
            dynamic json = JsonConvert.DeserializeObject(response);
            foreach (dynamic topic in json.topics)
                yield return Topic.CreateFromJson(topic);
        }

        private string GetRequestUrl(string relativeUrl)
        {
            return this.baseUrl + relativeUrl;
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
                request.Timeout = Config.NodeBB.ApiRequestTimeout;

                Logger.Debug("Sending forum {0} request to URL: {1}", method, request.RequestUri);

                try
                {
                    using (var response = request.GetResponse())
                    using (var stream = response.GetResponseStream())
                    {
                        Logger.Debug("Response received, response code was: {0}", GetResponseCode(response));
                        return new StreamReader(stream).ReadToEnd();
                    }
                }
                catch (TimeoutException tex)
                {
                    Logger.Debug("Timeout exception for {0} request to URL: {1}", method, request.RequestUri);
                    ForumHelper.PauseConnections(tex, 10);
                    throw;
                }
                catch (WebException wex)
                {
                    Logger.Debug("Web exception for {0} request to URL: {1}", method, request.RequestUri);
                    ForumHelper.PauseConnections(wex, 10);
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
                request.Timeout = Config.NodeBB.ApiRequestTimeout;
                request.ContentType = "application/x-www-form-urlencoded";

                Logger.Debug("Sending forum {0} request to URL: {1}", method, request.RequestUri);

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
                        Logger.Debug("Response received, response code was: {0}", GetResponseCode(response));
                        return new StreamReader(stream).ReadToEnd();
                    }
                }
                catch (TimeoutException tex)
                {
                    Logger.Debug("Timeout exception for {0} request to URL: {1}", method, request.RequestUri);
                    ForumHelper.PauseConnections(tex, 10);
                    throw;
                }
                catch (WebException wex)
                {
                    Logger.Debug("Web exception for {0} request to URL: {1}", method, request.RequestUri);
                    ForumHelper.PauseConnections(wex, 10);
                    throw ParseFirstError(wex);
                }
            }
        }

        private static Exception ParseFirstError(WebException wex)
        {
            Logger.Debug("Attempting to get and parse the first error returned from forum JSON result...");
            try
            {
                string jsonErrors = new StreamReader(wex.Response.GetResponseStream()).ReadToEnd();
                Logger.Debug("JSON result was: " + jsonErrors);
                wex.Response.Dispose();
                dynamic json = JsonConvert.DeserializeObject(jsonErrors);
                if (json == null)
                {
                    Logger.Debug("JSON error was null.");
                    return wex;
                }

                return new InvalidOperationException(json.errors[0].ToString(), wex);
            }
            catch
            {
                Logger.Debug("There was an error attempting to parse the first JSON error.");
                var ex = new InvalidOperationException("Unknown error connecting to the forum API. Ensure the host name is correct in web.config. The sidebar category must also exist on NodeBB.", wex);
                ForumHelper.PauseConnections(ex, 10);
                return ex;
            }
        }

        private static string GetResponseCode(WebResponse response)
        {
            var httpResponse = response as HttpWebResponse;
            if (httpResponse == null)
                return string.Empty;

            return string.Format("{0} ({1})", (int)httpResponse.StatusCode, httpResponse.StatusDescription);
        }
    }
}