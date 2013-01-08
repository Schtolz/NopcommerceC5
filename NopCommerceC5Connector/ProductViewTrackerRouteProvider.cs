using Nop.Web.Framework.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Web.Mvc;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class ProductViewTrackerRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Nop.Plugin.Other.ProductViewTracker.Log", "tracking/productviews/{productId}", new { controller = "Tracking", action = "Index" }, new[] { "Nop.Plugin.Other.ProductViewTracker.Controllers" });

            routes.MapRoute("Nop.Plugin.Other.NopCommerceC5Connector.Configure", "Plugins/NopCommerceC5Connector/Configure",
                            new { controller = "Settings", action = "Index" },
                            new[] { "Nop.Plugin.Other.NopCommerceC5Connector.Controllers" }
                );
        }
        public int Priority
        {
            get { return 0; }
        }
    }
}
