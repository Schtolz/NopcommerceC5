using Nop.Core.Plugins;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class NopCommerceC5ConnectorSynchronizationTask: ITask
    {
        private readonly IPluginFinder _pluginFinder;
        //private readonly IMailChimpApiService _mailChimpApiService;

        public NopCommerceC5ConnectorSynchronizationTask(IPluginFinder pluginFinder)
        {
            this._pluginFinder = pluginFinder;
            
        }

        /// <summary>
        /// Execute task
        /// </summary>
        public void Execute()
        {
            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("Other.NopCommerceC5Connector");
            if (pluginDescriptor == null)
                return;

            //is plugin configured?
            var plugin = pluginDescriptor.Instance() as NopCommerceC5ConnectorPlugin;
            if (plugin == null || !plugin.IsConfigured())
                return;

            //_mailChimpApiService.Synchronize();
        }
    }
}