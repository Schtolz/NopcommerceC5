using Autofac;
using Autofac.Core;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Other.NopCommerceC5Connector.Data;
using Nop.Plugin.Other.NopCommerceC5Connector.Domain;
using Nop.Plugin.Other.NopCommerceC5Connector.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac.Integration.Mvc;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class ProductViewTrackerDependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_NAME = "nop_object_context_product_view_tracker";

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //Load custom data settings
            var dataSettingsManager = new DataSettingsManager();
            DataSettings dataSettings = dataSettingsManager.LoadSettings();

            string nameOrConnectionString = null;

            if (dataSettings != null && dataSettings.IsValid())
            {
                //determine if the connection string exists
                nameOrConnectionString = dataSettings.DataConnectionString;
            }

            //Register custom object context
            builder.Register<IDbContext>(c => RegisterIDbContext(c, dataSettings)).Named<IDbContext>(CONTEXT_NAME).InstancePerHttpRequest();
            builder.Register(c => RegisterIDbContext(c, dataSettings)).InstancePerHttpRequest();

            //Register services
            builder.RegisterType<ViewTrackingService>().As<IViewTrackingService>();

            builder.RegisterType<NopCommerceC5ConnectorInstallationService>().AsSelf().InstancePerHttpRequest();

            //Override the repository injection
            builder.RegisterType<EfRepository<TrackingRecord>>().As<IRepository<TrackingRecord>>().WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_NAME)).InstancePerHttpRequest();
        }

        public int Order
        {
            get { return 0; }
        }
        /// <summary>
        /// Registers the I db context.
        /// </summary>
        /// <param name="componentContext">The component context.</param>
        /// <param name="dataSettings">The data settings.</param>
        /// <returns></returns>
        private TrackingRecordObjectContext RegisterIDbContext(IComponentContext componentContext, DataSettings dataSettings)
        {
            string dataConnectionStrings;

            if (dataSettings != null && dataSettings.IsValid())
            {
                dataConnectionStrings = dataSettings.DataConnectionString;
            }
            else
            {
                dataConnectionStrings = componentContext.Resolve<DataSettings>().DataConnectionString;
            }

            return new TrackingRecordObjectContext(dataConnectionStrings);
        }
    }
}
