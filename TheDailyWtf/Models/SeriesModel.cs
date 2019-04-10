using System;
using System.Collections.Generic;
using System.Linq;
using Inedo;
using TheDailyWtf.Data;

namespace TheDailyWtf.Models
{
    public sealed class SeriesModel
    {
        private static readonly LazyCached<List<SeriesModel>> allSeries = new LazyCached<List<SeriesModel>>(
            () => DB.Series_GetSeries().Select(FromTable).ToList(),
            TimeSpan.FromHours(2)
        );

        private static readonly Lazy<Dictionary<string, SeriesModel>> seriesMap = new Lazy<Dictionary<string, SeriesModel>>(() =>
        {
            return DB.Series_GetSeries()
                .ToDictionary(s => s.Title_Text, s => FromTable(s), StringComparer.OrdinalIgnoreCase);
        });

        internal static Dictionary<string, SeriesModel> LegacySeriesMap => seriesMap.Value;

        public string Slug { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string CssClass 
        { 
            get 
            {
                switch (this.Slug)
                {
                    case "feature-articles": return "featured";
                    case "code-sod": return "code";
                    case "errord": return "errord";
                    case "alexs-soapbox": return "tales";
                    default: return "soapbox";
                }
            } 
        }

        public static SeriesModel FromTable(Tables.Series series)
        {
            return new SeriesModel()
            {
                Slug = series.Series_Slug,
                Title = series.Title_Text,
                Description = series.Description_Text
            };
        }

        public static SeriesModel FromTable(Tables.Articles_Extended article)
        {
            return new SeriesModel()
            {
                Slug = article.Series_Slug,
                Title = article.Series_Title_Text,
                Description = article.Series_Description_Text
            };
        }

        public static SeriesModel GetSeriesBySlug(string slug)
        {
            var series = DB.Series_GetSeriesBySlug(slug);
            if (series == null)
                return null;
            return SeriesModel.FromTable(series);
        }

        public static IEnumerable<SeriesModel> GetAllSeries() => allSeries.Value;
    }
}