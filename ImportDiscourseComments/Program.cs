using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading;

namespace ImportDiscourseComments
{
    class Program
    {
        static void Main(string[] args)
        {
            var timeout = Convert.ToInt32(ConfigurationManager.AppSettings["Discourse.ApiRequestTimeout"]);
            var baseUrl = "https://" + ConfigurationManager.AppSettings["Discourse.Host"];

            using (var client = new HttpClient())
            using (SqlConnection writeConn = new SqlConnection(ConfigurationManager.AppSettings["InedoLib.DbConnectionString"]),
                                 readConn = new SqlConnection(ConfigurationManager.AppSettings["InedoLib.DbConnectionString"]))
            using (SqlCommand insertComment = writeConn.CreateCommand(), getArticles = readConn.CreateCommand())
            {
                readConn.Open();
                writeConn.Open();

                insertComment.CommandText = "INSERT INTO [Comments] ([Article_Id], [Body_Html], [User_Name], [Posted_Date], [Featured_Indicator], [User_IP], [User_Token], [Parent_Comment_Id]) " +
                    "OUTPUT INSERTED.[Comment_Id] VALUES (@Article_Id, @Body_Html, @User_Name, @Posted_Date, 'N', NULL, 'disco:' + CAST(@User_Id AS varchar(max)), @Parent_Comment_Id)";
                insertComment.CommandType = CommandType.Text;

                var Article_Id = insertComment.CreateParameter();
                Article_Id.ParameterName = "@Article_Id";
                Article_Id.SqlDbType = SqlDbType.Int;
                insertComment.Parameters.Add(Article_Id);

                var Body_Html = insertComment.CreateParameter();
                Body_Html.ParameterName = "@Body_Html";
                Body_Html.SqlDbType = SqlDbType.NVarChar;
                insertComment.Parameters.Add(Body_Html);

                var User_Name = insertComment.CreateParameter();
                User_Name.ParameterName = "@User_Name";
                User_Name.SqlDbType = SqlDbType.NVarChar;
                insertComment.Parameters.Add(User_Name);

                var Posted_Date = insertComment.CreateParameter();
                Posted_Date.ParameterName = "@Posted_Date";
                Posted_Date.SqlDbType = SqlDbType.DateTime;
                insertComment.Parameters.Add(Posted_Date);

                var User_Id = insertComment.CreateParameter();
                User_Id.ParameterName = "@User_Id";
                User_Id.SqlDbType = SqlDbType.Int;
                insertComment.Parameters.Add(User_Id);

                var Parent_Comment_Id = insertComment.CreateParameter();
                Parent_Comment_Id.ParameterName = "@Parent_Comment_Id";
                Parent_Comment_Id.SqlDbType = SqlDbType.Int;
                Parent_Comment_Id.IsNullable = true;
                insertComment.Parameters.Add(Parent_Comment_Id);

                getArticles.CommandText = "SELECT [Article_Id], [Discourse_Topic_Id] FROM [Articles] WHERE [Discourse_Topic_Opened] = 'Y'";
                getArticles.CommandType = CommandType.Text;
                using (var reader = getArticles.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int articleId = reader.GetInt32(0);
                        int discourseTopicId = reader.GetInt32(1);

                        Article_Id.Value = articleId;

                        var postNumberToCommentId = new Dictionary<int, int>();

                        int count = 0;
                        int page = 1;
                        string url = string.Format("{0}/t/{1}.json?include_raw=1", baseUrl, discourseTopicId);
                        while (url != null)
                        {
                            Console.WriteLine("Article {0} - GET {1}", articleId, url);
                            TopicResult topic;
                            using (var get = client.GetAsync(url).Result)
                            {
                                if (!get.IsSuccessStatusCode)
                                {
                                    Console.WriteLine("> Discourse returned {0} {1}. Waiting and trying again.", get.StatusCode, get.ReasonPhrase);
                                    Thread.Sleep(timeout);
                                    continue;
                                }

                                topic = JsonConvert.DeserializeObject<TopicResult>(get.Content.ReadAsStringAsync().Result);
                            }
                            foreach (var post in topic.PostStream.Posts)
                            {
                                count++;
                                if (post.UserName == ConfigurationManager.AppSettings["Discourse.Username"])
                                {
                                    continue;
                                }

                                Body_Html.Value = post.Raw;
                                User_Name.Value = post.UserName;
                                Posted_Date.Value = post.CreatedAt;
                                User_Id.Value = post.UserId;

                                if (post.ParentPostNumber != null && postNumberToCommentId.ContainsKey((int)post.ParentPostNumber))
                                {
                                    Parent_Comment_Id.Value = postNumberToCommentId[(int)post.ParentPostNumber];
                                }
                                else
                                {
                                    Parent_Comment_Id.Value = DBNull.Value;
                                }
                                using (var inserted = insertComment.ExecuteReader())
                                {
                                    if (inserted.Read())
                                    {
                                        postNumberToCommentId.Add(post.PostNumber, inserted.GetInt32(0));
                                        Console.WriteLine("> Added post #{0} by {1} as comment {2}", post.PostNumber, post.UserName, inserted.GetInt32(0));
                                    }
                                }
                            }

                            page++;
                            if (count <= topic.PostsCount)
                                url = string.Format("{0}/t/{1}.json?include_raw=1&page={2}", baseUrl, discourseTopicId, page);
                            else
                                url = null;
                        }
                    }
                }
            }
        }
    }

    struct TopicResult
    {
        [JsonProperty(PropertyName = "chunk_size")]
        public int ChunkSize { get; set; }

        [JsonProperty(PropertyName = "posts_count")]
        public int PostsCount { get; set; }

        [JsonProperty(PropertyName = "post_stream")]
        public PostStream PostStream { get; set; }
    }

    struct PostStream
    {
        [JsonProperty(PropertyName = "posts")]
        public IEnumerable<PostResult> Posts { get; set; }
    }

    struct PostResult
    {
        [JsonProperty(PropertyName = "raw")]
        public string Raw { get; set; }

        [JsonProperty(PropertyName = "post_number")]
        public int PostNumber { get; set; }

        [JsonProperty(PropertyName = "reply_to_post_number")]
        public int? ParentPostNumber { get; set; }

        [JsonProperty(PropertyName = "user_id")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
