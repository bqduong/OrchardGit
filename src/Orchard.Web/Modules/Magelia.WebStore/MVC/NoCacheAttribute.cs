using System.Web;
using System.Web.Mvc;

namespace Magelia.WebStore.MVC
{
    public class NoCacheAttribute : OutputCacheAttribute
    {
        public NoCacheAttribute()
        {
            this.Duration = 0;
            this.NoStore = true;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);
        }
    }
}