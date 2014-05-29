using System.Web.Mvc;
using TheDailyWtf.Models;

namespace TheDailyWtf.Controllers
{
    public class InfoController : Controller
    {
        //
        // GET: /Info/

        public ActionResult About()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Advertise()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Advertise(AdvertiserContactFormModel advertiser)
        {
            return RedirectToAction("advertise");
        }

        public ActionResult Privacy()
        {
            return View();
        }
    }
}
