using System.Web.Mvc;

namespace FGA
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new FGA.Filters.TraceAttribute());
        }
    }
}

