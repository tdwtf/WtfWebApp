using System;
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
            return Redirect("/contact");
        }

        public ActionResult Privacy()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult IHOC()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult ViewIhocGif(string imagename)
        {
            return Redirect("/Content/Images/IHOC/" + imagename + ".gif");
        }

        public ActionResult ViewIhocRotator()
        {
            var candidates = new string[] { "Obsolete_punchcards", "Obstacle_hlf_2x", "Orable_BetterThanOrable", "Orable_Ggggreat" };
            var rng = new Random();
            return ViewIhocGif(candidates[rng.Next(candidates.Length)]);
        }
    }
}
