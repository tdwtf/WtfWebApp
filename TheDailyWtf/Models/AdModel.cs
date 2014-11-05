using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TheDailyWtf.Data;

namespace TheDailyWtf.Models
{
    public class AdModel
    {
        public int Id { get; set; }
        [AllowHtml]
        public string BodyHtml { get; set; }

        public static IEnumerable<AdModel> GetAllFooterAds()
        {
            var ads = StoredProcs.Ads_GetAds().Execute();
            return ads.Select(ad => new AdModel { Id = ad.Ad_Id, BodyHtml = ad.Ad_Html });
        }

        public static AdModel GetFooterAdById(int id)
        {
            return GetAllFooterAds().FirstOrDefault(a => a.Id == id);
        }
    }
}