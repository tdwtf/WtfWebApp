using System;
using System.Web;
using System.Web.Mvc;
using Inedo.Diagnostics;
using TheDailyWtf.Data;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class AdsController : Controller
    {
        public ActionResult Index()
        {
            return View(new HomeIndexViewModel { ShowLeaderboardAd = false });
        }

        public ActionResult ViewAd(string id)
        {
            var ad = AdRotator.GetAdById(id);
            if (ad != null)
            {
                StoredProcs.AdImpressions_IncrementCount(ad.FileName, DateTime.Now.Date, 1).Execute();
                return File(ad.DiskPath, MimeMapping.GetMimeMapping(ad.DiskPath));
            }

            Logger.Error("Invalid Ad attempted to be loaded from: /fblast/{0}", id);
            return HttpNotFound();
        }

        public ActionResult ClickAd(string redirectGuid)
        {
            var url = AdRotator.GetOriginalUrlByRedirectGuid(redirectGuid);
            if (url != null)
            {
                StoredProcs.AdRedirectUrls_IncrementClickCount(Guid.Parse(redirectGuid), 1).Execute();
                return Redirect(url);
            }

            Logger.Error("Invalid Ad URL redirect GUID: {0}", redirectGuid);
            return HttpNotFound();
        }
    }
}
