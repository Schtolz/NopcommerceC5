﻿using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Other.NopCommerceC5Connector.Data;
using Nop.Plugin.Other.NopCommerceC5Connector.Services;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        private const string CONTEXT_DEPENDENCY_REGISTRY_KEY = "Nop.Plugin.Other.NopCommerceC5Connector-ObjectContext";

        /// <summary>
        /// Registers the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
        public void Register(ContainerBuilder builder, ITypeFinder typeFinder)
        {
            //builder.RegisterType<SubscriptionEventQueueingService>().As<ISubscriptionEventQueueingService>().InstancePerHttpRequest();
            builder.RegisterType<NopCommerceC5ConnectorInstallationService>().AsSelf().InstancePerHttpRequest();
            builder.RegisterType<ImportFabric>().As<IImportFabric>().InstancePerHttpRequest();

            builder.RegisterType<ProductsImportService>().AsSelf().InstancePerHttpRequest();
            builder.RegisterType<CustomersImportService>().AsSelf().InstancePerHttpRequest();

            //builder.RegisterType<MailChimpApiService>().As<IMailChimpApiService>().InstancePerHttpRequest();

            //Register object context
            RegisterObjectContext(builder);

            //Register repository
            /*builder.RegisterType<EfRepository<MailChimpEventQueueRecord>>().As<IRepository<MailChimpEventQueueRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(CONTEXT_DEPENDENCY_REGISTRY_KEY)).InstancePerHttpRequest();*/
        }

        /// <summary>
        /// Registers the object context.
        /// </summary>
        /// <param name="builder">The builder.</param>
        private void RegisterObjectContext(ContainerBuilder builder)
        {
            //Open the data settings manager
            var dataSettingsManager = new DataSettingsManager();
            var dataProviderSettings = dataSettingsManager.LoadSettings();

            string nameOrConnectionString = null;

            if (dataProviderSettings != null && dataProviderSettings.IsValid())
            {
                //determine if the connection string exists
                nameOrConnectionString = dataProviderSettings.DataConnectionString;
            }

            //Register the named instance
            builder.Register<IDbContext>(c => new TrackingRecordObjectContext(nameOrConnectionString ?? c.Resolve<DataSettings>().DataConnectionString))
                .Named<IDbContext>(CONTEXT_DEPENDENCY_REGISTRY_KEY).InstancePerHttpRequest();

            //Register the type
            builder.Register(c => new TrackingRecordObjectContext(nameOrConnectionString ?? c.Resolve<DataSettings>().DataConnectionString)).InstancePerHttpRequest();
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order
        {
            get { return 0; }
        }
    }
}