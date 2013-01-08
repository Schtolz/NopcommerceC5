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
                    return new CustomersImportService();
                case Utility.Utility.ImportType.Products:
                    return new ProductsImportService();
                default:
                    return null;
            }
        }
    }
}
