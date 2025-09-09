using System.Web.Mvc;
using IKart_ClientSide.Filters;

namespace IKart_ClientSide
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AdminAuthorizeAttribute()); // apply to all
        }
    }
}
