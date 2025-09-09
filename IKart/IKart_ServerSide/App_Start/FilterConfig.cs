using System.Web.Mvc;

namespace IKart_ClientSide
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            // Removed global AdminAuthorizeAttribute
        }
    }
}