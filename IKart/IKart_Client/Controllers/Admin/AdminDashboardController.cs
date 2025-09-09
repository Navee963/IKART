using IKart_ClientSide.Filters;
using System.Web.Mvc;

namespace IKart_ClientSide.Controllers.Admin
{
    [AdminAuthorize]
    public class AdminDashboardController : Controller
    {
        public ActionResult Index()
        {
            if (Session["AdminId"] == null)
                return RedirectToAction("Login", "AdminAuth");

            ViewBag.Username = Session["AdminUsername"];
            return View();
        }
    }
}
