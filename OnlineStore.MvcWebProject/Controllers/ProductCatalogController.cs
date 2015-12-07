using System.Web.Mvc;

namespace OnlineStore.MvcWebProject.Controllers
{
    [Authorize(Roles = "User")]
    public class ProductCatalogController : Controller
    {
        [HttpGet]
        public ActionResult ProductList()
        {
            return View();
        }

    }
}
