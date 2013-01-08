using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Nop.Plugin.Other.NopCommerceC5Connector.Configure", "Plugins/NopCommerceC5Connector/Configure",
                            new { controller = "Settings", action = "Index" },
                            new[] { "Nop.Plugin.Other.NopCommerceC5Connector.Controllers" }
                );

            routes.MapRoute("Nop.Plugin.Other.NopCommerceC5Connector.WebHook", "Plugins/NopCommerceC5Connector/WebHook/{webHookKey}",
                new { controller = "WebHooks", action = "index" },
                new[] { "Nop.Plugin.Other.NopCommerceC5Connector.Controllers" });
        }

        public int Priority
        {
            get { return 0; }
        }

    }
}
