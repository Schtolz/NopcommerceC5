using Nop.Core.Plugins;
using Nop.Plugin.Other.NopCommerceC5Connector.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class ProductViewTrackerPlugin : BasePlugin
    {
        private readonly TrackingRecordObjectContext _context;

        public ProductViewTrackerPlugin(TrackingRecordObjectContext context)
        {
            _context = context;
        }

        public override void Install()
        {
            _context.Install();
            base.Install();
        }

        public override void Uninstall()
        {
            _context.Uninstall();
            base.Uninstall();
        }
    }
}
