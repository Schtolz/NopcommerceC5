using CsvHelper;
using Nop.Core.Plugins;
using Nop.Plugin.Other.NopCommerceC5Connector.Services;
using Nop.Services.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class C5Order
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public string OrderHistoryId { get; set; }
        public double Price { get; set; }
        public string Name { get; set; }
    }

    public class NopCommerceC5ConnectorPlugin : BasePlugin, IMiscPlugin
    {
        public void Test(){
            using (var reader = new StreamReader(@"d:\projects\Hp Marsking\test.csv"))
            {
                var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration() {  Delimiter=';', Quote='"', HasHeaderRecord=false});
                var actorsList = csv.GetRecords<C5Order>();
            }
        }

        private readonly NopCommerceC5ConnectorInstallationService _nopCommerceC5ConnectorInstallationService;
        private readonly NopCommerceC5ConnectorSettings _nopCommerceC5ConnectorSettings;

        public NopCommerceC5ConnectorPlugin(NopCommerceC5ConnectorInstallationService nopCommerceC5ConnectorInstallationService,
            NopCommerceC5ConnectorSettings nopCommerceC5ConnectorSettings)
        {
            this._nopCommerceC5ConnectorInstallationService = nopCommerceC5ConnectorInstallationService;
            this._nopCommerceC5ConnectorSettings = nopCommerceC5ConnectorSettings;
        }

        /// <summary>
        /// Is plugin configured?
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConfigured()
        {
            return !string.IsNullOrEmpty(_nopCommerceC5ConnectorSettings.ApiKey);
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _nopCommerceC5ConnectorInstallationService.Install(this);
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            _nopCommerceC5ConnectorInstallationService.Uninstall(this);
            base.Uninstall();
        }

        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Index";
            controllerName = "Settings";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Other.NopCommerceC5Connector.Controllers" }, { "area", null } };
        }
    }
}
