using System.Web.Mvc;
using TheDailyWtf.Models;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class InfoController : WtfControllerBase
    {
        //
        // GET: /Info/

        public ActionResult About()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Advertise()
        {
            return View(new HomeIndexViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Advertise(AdvertiserContactFormModel advertiser)
        {
            // send contact email...

            return RedirectToAction("advertise");
        }

        public ActionResult Privacy()
        {
            return View(new HomeIndexViewModel());
        }
    }
}
