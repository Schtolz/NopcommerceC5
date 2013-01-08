using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Plugins;
using Nop.Services.Customers;
using Nop.Plugin.Other.NopCommerceC5Connector.Domain;
using Nop.Plugin.Other.NopCommerceC5Connector.Services;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Controllers
{
    public class TrackingController : Controller
    {
        private readonly IProductService _productService;
        private readonly IViewTrackingService _viewTrackingService;
        private readonly IWorkContext _workContext;

        public TrackingController(IWorkContext workContext,
            IViewTrackingService viewTrackingService,
            IProductService productService,
            IPluginFinder pluginFinder)
        {
            _workContext = workContext;
            _viewTrackingService = viewTrackingService;
            _productService = productService;
        }

        [ChildActionOnly]
        public ActionResult Index(int productId)
        {
            //Read from the product service
            Product productById = _productService.GetProductById(productId);

            //If the product exists we will log it
            if (productById != null)
            {
                //Setup the product to save
                var record = new TrackingRecord();
                record.ProductId = productId;
                record.ProductName = productById.Name;
                record.CustomerId = _workContext.CurrentCustomer.Id;
                record.IpAddress = _workContext.CurrentCustomer.LastIpAddress;
                record.IsRegistered = _workContext.CurrentCustomer.IsRegistered();

                //Map the values we're interested in to our new entity
                _viewTrackingService.Log(record);
            }

            //Return the view, it doesn't need a model
            return Content("");
        }
    }
}
