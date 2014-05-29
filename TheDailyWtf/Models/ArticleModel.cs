using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Configuration;
using Inedo;
using TheDailyWtf.Data;

namespace TheDailyWtf.Models
{
    public sealed class ArticleModel
    {
        public int Id { get; set; }
        public AuthorModel Author { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public string Body { get; set; }
        public string Title { get; set; }
        public int CommentCount { get; set; }
        public DateTime LastCommentDate { get; set; }
        public string LastCommentRelativeDate { get; set; }
        public int? DiscourseTopicId { get; set; }
        public string DiscourseThreadUrl { get { return string.Format("http://what.thedailywtf.com/t/{0}", this.DiscourseTopicId); } }
        public DateTime? PublishedDate { get; set; }
        public SeriesModel Series { get; set; }
        public string Url { get { return string.Format("//{0}/articles/{1}", WebConfigurationManager.AppSettings["Wtf.Host"], this.Slug); } }
        public string Slug { get { return this.Title.Replace(" ", "-").ToLower(); } }

        public static IEnumerable<ArticleModel> GetAllArticlesByMonth(DateTime month)
        {
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var articles = StoredProcs.Articles_GetArticles(
                null,
                Domains.PublishedStatus.Published,
                monthStart,
                monthStart.AddMonths(1).AddSeconds(-1.0)
             ).Execute();

            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetSeriesArticlesByMonth(string series, DateTime month)
        {
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var articles = StoredProcs.Articles_GetArticles(
                series, 
                Domains.PublishedStatus.Published, 
                monthStart, 
                monthStart.AddMonths(1).AddSeconds(-1.0)
              ).Execute();

            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetRecentArticles()
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Article_Count: 8).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetRecentArticlesBySeries(string slug)
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Series_Slug: slug, Article_Count: 8).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public static IEnumerable<ArticleModel> GetRecentArticlesByAuthor(string slug)
        {
            var articles = StoredProcs.Articles_GetRecentArticles(Domains.PublishedStatus.Published, Author_Slug: slug, Article_Count: 8).Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public IEnumerable<ArticleModel> GetSimilarArticles()
        {
            yield return GetArticleBySlug("but-the-tests-prove-that-hdars-works-correctly");
            yield return GetArticleBySlug("but-the-tests-prove-that-hdars-works-correctly2");
            yield return GetArticleBySlug("but-the-tests-prove-that-hdars-works-correctly3");
            yield return GetArticleBySlug("but-the-tests-prove-that-hdars-works-correctly4");
            yield return GetArticleBySlug("but-the-tests-prove-that-hdars-works-correctly");
        }

        public static IEnumerable<ArticleModel> GetUnpublishedArticles()
        {
            var articles = StoredProcs.Articles_GetUnpublishedArticles().Execute();
            return articles.Select(a => ArticleModel.FromTable(a));
        }

        public IEnumerable<CommentModel> GetFeaturedComments()
        {
            return CommentModel.GetFeaturedCommentsForArticle(this.Id);
        }

        public static ArticleModel GetArticleBySlug(string slug)
        {
            var article = StoredProcs.Articles_GetArticleBySlug(slug).Execute();
            return ArticleModel.FromTable(article);
        }

        public static ArticleModel FromTable(Tables.Articles_Extended article)
        {
            DateTime lastCommentDate = DateTime.Now;
            return new ArticleModel()
            {
                Author = AuthorModel.FromTable(article),
                Body = article.Body_Html,
                CommentCount = 1000,
                DiscourseTopicId = article.Discourse_Topic_Id,
                Id = article.Article_Id,
                LastCommentDate = lastCommentDate,
                LastCommentRelativeDate = InedoLib.Util.DateTime.RelativeDate(DateTime.Now, lastCommentDate),
                PublishedDate = article.Published_Date,
                Series = SeriesModel.FromTable(article),
                Status = article.PublishedStatus_Name,
                Summary = article.Body_Html,
                Title = article.Title_Text
            };
        }

        private static ArticleModel CreateStubArticle()
        {
            return new ArticleModel()
            {
                Body =
               @"<p>I. G. wrote about an incident that caused him to nearly give himself a concussion from a *headdesk* moment. A newly developed system was meticulously designed, coded and tested to obscene levels; all appeared well.</p>
    <p>Unfortunately, upon deployment, it began acting erratically, returning incorrect results from numerous database queries. After many debugging sessions and code walkthroughs, it was discovered that the developers had used the following pattern for all the database DAO tests:</p>
    <h4>Articles can contain Subheaders</h4>
    <p>When queried, the developers said that the tests took too long to run when they hit the database, so they created stub-DAO's to return canned data, because they assumed the SQL queries would be correct and didn't need testing.</p>
    <p>The elevator opened on a floor with a bright, contemporary look- the intersection of Ten Forward and Office Space. Brett led Eric around a corner just in time to see a large man in an expensive three-piece suit put his fist through the wall. His face was the bright red of <strong>alarm beacons</strong>. Curses and spittle flew from his mouth.</p>
<h4>Codeblocks can be hidden by default</h4>

                    <p class=""btn medium primary"">
                        <a href=""#"" class=""toggle"" gumby-trigger="".codeBlock"">Show<i class=""icon-eye""></i>Codeblock</a>
                    </p>

                    <div class=""drawer codeBlock"">
                        <pre class=""language-javascript"" data-src=""codeExample.class"">
                            <code class=""language-class"">
class MyDateString { 
  private static Calendar cal = Calendar.getInstance(); 

  public MyDateString() {} 

  public static final long getDateString(long currtime) { 
    if (currtime &lt; 1000000) return -1L; 
    long rv = (currtime % 1000L); 
    cal.setTimeInMillis(currtime);
    rv += 1000L*((long)cal.get(Calendar.SECOND)); 
    rv += 100000L*((long)cal.get(Calendar.MINUTE)); 
    rv += 10000000L*((long)cal.get(Calendar.HOUR_OF_DAY)); 
    rv += 1000000000L*((long)cal.get(Calendar.DAY_OF_MONTH)); 
    rv += 100000000000L*((long)(1+cal.get(Calendar.MONTH))); 
    rv += 10000000000000L*((long)cal.get(Calendar.YEAR)); 
    return rv; 
  } 
}

                            </code>
                        </pre>
                    </div>

                    <h4>Images floating with text</h4>
                    <div class=""imageContainer imgFloating"">
                        <img src=""/content/images/placeholder.png"">
                    </div>

                    <p>""Keep that [BLEEP]ing [BLEEP] at home, Bob!"" the giant roared. ""You [BLEEP]ing hear me?"" </p>
                    <p>Like a Vietnam vet shepherding the FNG, Brett backed himself and Eric into a spot where they could observe safely. The cubicle rows in the vicinity were dotted with the wide eyes of cowering spectators, all wary of drawing attention to themselves. Bob stood alone against the onslaught, stooped and cowering. He clutched an external hard drive against his chest as though it were a shield. </p>
                    <p>When queried, the developers said that the tests took too long to run when they hit the database, so they created stub-DAO's to return canned data, because they assumed the SQL queries would be correct and didn't need testing.</p>

                    <h4>Full width images</h4>
                    <div class=""imageContainer"">
                        <img src=""/content/images/placeholder.png"">
                    </div>",

                Id = 1000,
                Author = AuthorModel.GetAuthorBySlug("alex-papadimoulis"),
                Title = "But the Tests Prove that Hdars Works Correctly",
                CommentCount = 1000,
                LastCommentDate = DateTime.Now,
                LastCommentRelativeDate = "1000 days ago",
                PublishedDate = DateTime.Now,
                Series = SeriesModel.GetSeriesBySlug("feature-articles"),
                Summary = @"I. G. wrote about an incident that caused him to nearly give himself a concussion from a *headdesk* moment. A newly developed system was meticulously designed, coded and tested to obscene levels; all appeared well.
    Unfortunately, upon deployment, it began acting erratically, returning incorrect results from numerous database queries. After many debugging sessions and code walkthroughs, it was discovered that the developers had used the following pattern for all the database DAO tests:"
            };
        }
    }
}