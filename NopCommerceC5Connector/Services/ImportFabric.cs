using Nop.Core.Infrastructure;
using Nop.Plugin.Other.NopCommerceC5Connector.Services.ImportServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services
{
    public class ImportFabric: IImportFabric
    {
        public IImportService GetImportService(Utility.Utility.ImportType importType)
        {
            switch (importType)
            {
                case Utility.Utility.ImportType.Customers:
                    return (ImportService)EngineContext.Current.Resolve<CustomersImportService>();
                case Utility.Utility.ImportType.Products:
                    return (ImportService)EngineContext.Current.Resolve<ProductsImportService>();
                case Utility.Utility.ImportType.Discounts:
                    return (ImportService)EngineContext.Current.Resolve<DiscountImportService>();
                default:
                    return null;
            }
        }
    }
}
