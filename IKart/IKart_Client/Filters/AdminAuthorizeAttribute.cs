using System.Web;
using System.Web.Mvc;

namespace IKart_ClientSide.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var route = httpContext.Request.RequestContext.RouteData;
            string controller = route.Values["controller"].ToString();
            string action = route.Values["action"].ToString();

            // Allow AdminAuth/Login without session
            if (controller.Equals("AdminAuth", System.StringComparison.OrdinalIgnoreCase) &&
                action.Equals("Login", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return httpContext.Session["AdminId"] != null;
        }


        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Redirect to Admin Login page
            filterContext.Result = new RedirectResult("~/AdminAuth/Login");
        }
    }
}
