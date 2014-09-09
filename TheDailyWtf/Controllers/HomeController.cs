using System.Linq;
using System.Web.Mvc;
using Inedo.Web;
using TheDailyWtf.Models;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class HomeController : WtfControllerBase
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Search()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Contact()
        {
            return View(new HomeIndexViewModel());
        }

        public ActionResult Sponsors()
        {
            return View(new HomeIndexViewModel());
        }

        [HttpPost]
        public ActionResult Contact(ContactFormModel contact)
        {
            // send submit WTF or contact email...

            return RedirectToAction("contact");
        }
    }
}
