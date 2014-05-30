using System.Web.Mvc;
using TheDailyWtf.Models;
using TheDailyWtf.ViewModels;

namespace TheDailyWtf.Controllers
{
    public class HomeController : Controller
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

        [HttpPost]
        public ActionResult Contact(ContactFormModel contact)
        {
            // send submit WTF or contact email...

            return RedirectToAction("contact");
        }
    }
}
