using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services
{
    public interface IImportService
    {
        void Import(HttpPostedFileBase file);
    }
}
