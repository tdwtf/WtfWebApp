using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FixDiscourseComments
{
    class Program
    {
        static void Main(string[] args)
        {
            var WtfHardcodedUser = 9;
            var WtfHardcodedTopics = new Dictionary<int, int>()
            {
                [18444] = 8341, // http://thedailywtf.com/articles/tdwtf-api
                [14803] = 8073, // http://thedailywtf.com/articles/limit-as-sense-approaches-zeno
                [16727] = 8216, // http://thedailywtf.com/articles/what-is-this-right-click-you-speak-of
                [16379] = 8161, // http://thedailywtf.com/articles/best-of-email-super-spam-edition
                [16337] = 8181, // http://thedailywtf.com/articles/the-worst-boss-ever
                [16280] = 8175, // http://thedailywtf.com/articles/you-ve-been-warned
                [16321] = 8180, // http://thedailywtf.com/articles/last-chance-to-back-programming-languages-abc-
                [16185] = 8168, // http://thedailywtf.com/articles/it-s-a-kinda-magic
                [16007] = 8157, // http://thedailywtf.com/articles/recruiting-desperation
                [15944] = 8152, // http://thedailywtf.com/articles/fired-up
                [15800] = 8140, // http://thedailywtf.com/articles/used-shellfish
                [14682] = 8061, // http://thedailywtf.com/articles/getting-wired
                [15478] = 8115, // http://thedailywtf.com/articles/the-monolith
                [15444] = 8111, // http://thedailywtf.com/articles/phenomenesia
                [15212] = 8097, // http://thedailywtf.com/articles/pretty-please-
                [14698] = 8065, // http://thedailywtf.com/articles/accurate-comments
                [14711] = 8066, // http://thedailywtf.com/articles/an-interesting-cryptic-and-artistic-error
                [14492] = 8056, // http://thedailywtf.com/articles/you-can-t-beat-this-high-score
                [14110] = 8019, // http://thedailywtf.com/articles/metro-card-jackpot-
                [14235] = 8017, // http://thedailywtf.com/articles/jack-and-the-beanstalk
                [14223] = 8013, // http://thedailywtf.com/articles/papers-please
                [14270] = 8039, // http://thedailywtf.com/articles/savings-that-you-wouldn-t-believe
                [14210] = 8034, // http://thedailywtf.com/articles/everything-californian-be-found-on-the-internet
                [13506] = 7969, // http://thedailywtf.com/articles/tokyo-meet-up-site-fixes
                [13933] = 7999, // http://thedailywtf.com/articles/classic-wtf-the-defect-black-market
            };

            IEnumerable<TopicPosts> topics;
            using (var reader = new StreamReader("articles.json", Encoding.UTF8))
            {
                topics = JsonConvert.DeserializeObject<IEnumerable<TopicPosts>>(reader.ReadToEnd());
            }
            var articleIdPattern = new Regex("<!--ARTICLEID:([0-9]+)-->", RegexOptions.Compiled);

            using (var conn = new SqlConnection(ConfigurationManager.AppSettings["InedoLib.DbConnectionString"]))
            using (SqlCommand getComments = conn.CreateCommand(), insertComment = conn.CreateCommand())
            {
                conn.Open();

                var getComments_Article_Id = getComments.CreateParameter();
                getComments_Article_Id.ParameterName = "@Article_Id";
                getComments_Article_Id.SqlDbType = SqlDbType.Int;
                getComments.Parameters.Add(getComments_Article_Id);

                getComments.CommandText = "SELECT [Comment_Id], [Parent_Comment_Id], [Posted_Date], [Body_Html], [User_Name], [User_Token] FROM [Comments] WHERE [Article_Id] = @Article_Id AND [User_Token] LIKE 'disco:%' ORDER BY [Posted_Date] ASC";
                getComments.CommandType = CommandType.Text;

                var Article_Id = insertComment.CreateParameter();
                Article_Id.ParameterName = "@Article_Id";
                Article_Id.SqlDbType = SqlDbType.Int;
                insertComment.Parameters.Add(Article_Id);

                var Parent_Comment_Id = insertComment.CreateParameter();
                Parent_Comment_Id.ParameterName = "@Parent_Comment_Id";
                Parent_Comment_Id.SqlDbType = SqlDbType.Int;
                insertComment.Parameters.Add(Parent_Comment_Id);

                var Posted_Date = insertComment.CreateParameter();
                Posted_Date.ParameterName = "@Posted_Date";
                Posted_Date.SqlDbType = SqlDbType.DateTime;
                insertComment.Parameters.Add(Posted_Date);

                var Body_Html = insertComment.CreateParameter();
                Body_Html.ParameterName = "@Body_Html";
                Body_Html.SqlDbType = SqlDbType.NVarChar;
                insertComment.Parameters.Add(Body_Html);

                var User_Name = insertComment.CreateParameter();
                User_Name.ParameterName = "@User_Name";
                User_Name.SqlDbType = SqlDbType.NVarChar;
                insertComment.Parameters.Add(User_Name);

                var User_Token = insertComment.CreateParameter();
                User_Token.ParameterName = "@User_Token";
                User_Token.SqlDbType = SqlDbType.VarChar;
                insertComment.Parameters.Add(User_Token);

                insertComment.CommandText = "INSERT INTO [Comments] ([Article_Id], [Parent_Comment_Id], [Posted_Date], [Body_Html], [User_Name], [User_Token], [Featured_Indicator]) OUTPUT INSERTED.[Comment_Id] VALUES(@Article_Id, @Parent_Comment_Id, @Posted_Date, @Body_Html, @User_Name, @User_Token, 'N')";
                insertComment.CommandType = CommandType.Text;

                foreach (var topic in topics)
                {
                    int articleId;
                    var match = articleIdPattern.Match(topic.Posts.First().Post.Content);
                    if (match.Success)
                    {
                        articleId = Convert.ToInt32(match.Groups[1].Value);
                    }
                    else if (WtfHardcodedTopics.ContainsKey(topic.Topic.Id))
                    {
                        articleId = WtfHardcodedTopics[topic.Topic.Id];
                    }
                    else
                    {
                        Console.WriteLine("no article found in topic {0}: {1}", topic.Topic.Slug, topic.Topic.Title);
                        continue;
                    }
                    Console.WriteLine("getting comments for article {0} ({1})", articleId, topic.Topic.Title);

                    getComments_Article_Id.Value = articleId;

                    var replies = topic.Posts.Skip(1).OrderBy(p => p.Post.Id).Where(p => p.User.Id != WtfHardcodedUser && !p.Post.IsDeleted);
                    var comments = new Dictionary<int, Comment>();

                    using (var reader = getComments.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var comment = new Comment()
                            {
                                Id = reader.GetInt32(0),
                                ParentId = ToNullable(reader.GetSqlInt32(1)),
                                PostedDate = reader.GetDateTime(2),
                                BodyHtml = reader.GetString(3),
                                UserName = reader.GetString(4),
                                UserToken = reader.GetString(5),
                            };

                            comments[replies.First(p => p.User.Name == comment.UserName && p.Post.Content == comment.BodyHtml).Post.Id] = comment;
                        }
                    }

                    Console.WriteLine("article has {0} comments, topic has {1} replies", comments.Count, replies.Count());

                    foreach (var reply in replies)
                    {
                        if (comments.ContainsKey(reply.Post.Id))
                        {
                            continue;
                        }

                        var comment = new Comment()
                        {
                            PostedDate = reply.Post.PostedAt,
                            BodyHtml = reply.Post.Content,
                            UserName = reply.User.Name,
                            UserToken = $"disco:{reply.User.ImportedId}",
                        };

                        if (reply.Post.ParentId != null && comments.ContainsKey(reply.Post.ParentId.Value))
                        {
                            comment.ParentId = comments[reply.Post.ParentId.Value].Id;
                        }

                        Article_Id.Value = articleId;
                        Parent_Comment_Id.SqlValue = comment.ParentId.HasValue ? new SqlInt32(comment.ParentId.Value) : SqlInt32.Null;
                        Posted_Date.Value = comment.PostedDate;
                        Body_Html.Value = comment.BodyHtml;
                        User_Name.Value = comment.UserName;
                        User_Token.Value = comment.UserToken;
                        comment.Id = Convert.ToInt32(insertComment.ExecuteScalar());

                        comments[reply.Post.Id] = comment;
                    }
                }
            }
        }

        internal static int? ToNullable(SqlInt32 sql)
        {
            return sql.IsNull ? (int?)null : sql.Value;
        }

        struct TopicPosts
        {
            [JsonProperty(PropertyName = "topic")]
            public Topic Topic { get; set; }

            [JsonProperty(PropertyName = "posts")]
            public IEnumerable<PostUser> Posts { get; set; }
        }

        struct Topic
        {
            [JsonProperty(PropertyName = "tid")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }

            [JsonProperty(PropertyName = "slug")]
            public string Slug { get; set; }
        }

        struct PostUser
        {
            [JsonProperty(PropertyName = "post")]
            public Post Post { get; set; }

            [JsonProperty(PropertyName = "user")]
            public User User { get; set; }
        }

        struct Post
        {
            [JsonProperty(PropertyName = "pid")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "toPid")]
            public int? ParentId { get; set; }

            [JsonProperty(PropertyName = "deleted")]
            public int Deleted { get; set; }

            public bool IsDeleted { get { return this.Deleted != 0; } }

            [JsonProperty(PropertyName = "timestamp")]
            public long Timestamp { get; set; }

            private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            public DateTime PostedAt { get { return epoch + TimeSpan.FromMilliseconds(this.Timestamp); } }

            [JsonProperty(PropertyName = "content")]
            public string Content { get; set; }
        }

        struct User
        {
            [JsonProperty(PropertyName = "uid")]
            public int Id { get; set; }

            [JsonProperty(PropertyName = "username")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "_imported_uid")]
            public int ImportedId { get; set; }
        }

        struct Comment
        {
            public int Id { get; set; }
            public int? ParentId { get; set; }
            public DateTime PostedDate { get; set; }
            public string BodyHtml { get; set; }
            public string UserName { get; set; }
            public string UserToken { get; set; }
        }
    }
}
