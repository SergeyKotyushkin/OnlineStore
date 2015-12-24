using System.Web.Mvc;

namespace OnlineStore.MvcWebProject.Controllers
{
    public class ErrorsController : Controller
    {
        public ActionResult BadRequest()
        {
            object model = "Bad Request";
            return View(model);
        }

        public ActionResult Unauthorized()
        {
            object model = "Unauthorized";
            return View(model);
        }

        public ActionResult Forbidden()
        {
            object model = "Forbidden";
            return View(model);
        }

        public ActionResult NotFound()
        {
            object model = "Not Found";
            return View(model);
        }

        public ActionResult RequestTimeout()
        {
            object model = "Request Timeout";
            return View(model);
        }

        public ActionResult InternalServerError()
        {
            object model = "Internal Server Error";
            return View(model);
        }

        public ActionResult UnhandledServerError()
        {
            object model = "Unhandled Server Error";
            return View(model);
        }
    }
}
