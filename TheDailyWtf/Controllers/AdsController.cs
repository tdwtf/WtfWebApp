using System.Web;
using System.Web.Mvc;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class AdsController : Controller
    {
        public ActionResult Index()
        {
            return View(new HomeIndexViewModel { ShowLeaderboardAd = false });
        }

        [OutputCache(CacheProfile = CacheProfile.Timed5Minutes)]
        public ActionResult ViewAd(string id)
        {
            var ad = AdRotator.GetAdById(id);
            return File(ad.DiskPath, MimeMapping.GetMimeMapping(ad.DiskPath));
        }
    }
}
