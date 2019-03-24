using System;
using System.Collections.Generic;
using System.Linq;
using TheDailyWtf.Data;

namespace TheDailyWtf.ViewModels
{
    public sealed class ViewAdsViewModel : HomeIndexViewModel
    {
        private IEnumerable<Tables.AdImpressions> impressions;

        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }

        public string StartValue { get { return this.Start == null ? "" : this.Start.Value.ToShortDateString(); } }
        public string EndValue { get { return this.End == null ? "" : this.End.Value.ToShortDateString(); } }

        public ViewAdsViewModel(DateTime? start, DateTime? end)
        {
            this.ShowLeaderboardAd = false;
            DateTime now = DateTime.Now;
            this.Start = start ?? new DateTime(now.Year, now.Month, 1);
            this.End = end ?? new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
            this.impressions = DB
                .AdImpressions_GetImpressions(this.Start, this.End)
                .OrderBy(i => i.Impression_Date)
                .ThenBy(i => i.Banner_Name);
        }

        public IEnumerable<Tables.AdRedirectUrls> GetAdClicks()
        {
            return DB.AdRedirectUrls_GetRedirectUrls().OrderBy(url => url.Redirect_Url);
        }

        public IEnumerable<Tables.AdImpressions> GetAdImpressions()
        {
            return this.impressions;
        }

        public IEnumerable<Tables.AdImpressions> GetTotalAdImpressions()
        {
            return this.impressions
                .GroupBy(i => i.Banner_Name)
                .Select(g => new Tables.AdImpressions { Banner_Name = g.Key, Impression_Count = g.Sum(r => r.Impression_Count) });
        }
    }
}