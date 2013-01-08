using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services
{
    public interface IImportFabric
    {
        IImportService GetImportService(Utility.Utility.ImportType importType);
    }
}
