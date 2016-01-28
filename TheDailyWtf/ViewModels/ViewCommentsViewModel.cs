﻿using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Models;

namespace TheDailyWtf.ViewModels
{
    public class ViewCommentsViewModel : WtfViewModelBase
    {
        public const int CommentsPerPage = 20;
        public const int NearbyPages = 3;

        public ViewCommentsViewModel(ArticleModel article, int page)
        {
            this.Article = article;
            this.Comments = CommentModel.FromArticle(article).Skip((page - 1) * CommentsPerPage).Take(CommentsPerPage);
            this.PageNumber = page;
            if (this.Article.CachedCommentCount > 1)
                this.DiscourseNextUnreadCommentUrl = this.Article.DiscourseThreadUrl + "/" + this.Article.CachedCommentCount;
            else
                this.DiscourseNextUnreadCommentUrl = this.Article.DiscourseThreadUrl;
        }

        public ViewCommentsViewModel(ArticleModel article, IEnumerable<CommentModel> comments)
        {
            this.Article = article;
            this.Comments = comments;
            this.PageNumber = -1;
        }

        public virtual string BaseUrl { get { return Article.CommentsUrl; } }
        public virtual bool CanFeature { get { return false; } }
        public virtual bool CanEditDelete { get { return false; } }
        public ArticleModel Article { get; private set; }
        public IEnumerable<CommentModel> Comments { get; protected set; }
        public int PageNumber { get; private set; }
        public int MaxDiscoursePostId { get; private set; }
        public string DiscourseNextUnreadCommentUrl { get; private set; }
        public int PageCount
        {
            get
            {
                return (this.Article.CachedCommentCount + CommentsPerPage - 1) / CommentsPerPage;
            }
        }
        public string ViewCommentsHeading 
        { 
            get 
            {
                return string.Format(
                    "Article Comments ({0} {1} comments)", 
                    this.Article.CachedCommentCount < this.Article.DiscourseCommentCount ? "Previewing first" : "Viewing", 
                    this.CommentsFraction
                ); 
            }
        }
        public string CommentsFraction
        {
            get
            {
                if (this.Article.CachedCommentCount < this.Article.DiscourseCommentCount)
                    return string.Format("{0} of {1}", this.Article.CachedCommentCount, this.Article.DiscourseCommentCount);
                else if (this.Comments.Count() < this.Article.CachedCommentCount)
                    return string.Format("{0} of {1}", this.Comments.Count(), this.Article.CachedCommentCount);
                else
                    return this.Article.CachedCommentCount.ToString();
            }
        }
    }
}