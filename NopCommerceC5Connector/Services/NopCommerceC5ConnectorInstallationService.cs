using Nop.Core.Domain.Tasks;
using Nop.Core.Plugins;
using Nop.Plugin.Other.NopCommerceC5Connector.Data;
using Nop.Services.Configuration;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Localization;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services
{
    public class NopCommerceC5ConnectorInstallationService
    {
        private readonly TrackingRecordObjectContext _trackingObjectContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;

        public NopCommerceC5ConnectorInstallationService(TrackingRecordObjectContext trackingObjectContext,
            IScheduleTaskService scheduleTaskService, ISettingService settingService)
        {
            this._trackingObjectContext = trackingObjectContext;
            this._scheduleTaskService = scheduleTaskService;
            this._settingService = settingService;
        }

        /// <summary>
        /// Installs the sync task.
        /// </summary>
        private void InstallSyncTask()
        {
            //Check the database for the task
            var task = FindScheduledTask();

            if (task == null)
            {
                task = new ScheduleTask
                {
                    Name = "NopCommerceC5Connector sync",
                    //each 60 minutes
                    Seconds = 3600,
                    Type = "Nop.Plugin.Other.NopCommerceC5Connector.NopCommerceC5ConnectorSynchronizationTask, Nop.Plugin.Other.NopCommerceC5Connector",
                    Enabled = false,
                    StopOnError = false,
                };
                _scheduleTaskService.InsertTask(task);
            }
        }

        private ScheduleTask FindScheduledTask()
        {
            return _scheduleTaskService.GetTaskByType("Nop.Plugin.Other.NopCommerceC5Connector.NopCommerceC5ConnectorSynchronizationTask, Nop.Plugin.Other.NopCommerceC5Connector");
        }

        /// <summary>
        /// Installs this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Install(BasePlugin plugin)
        {
            //settings
            var settings = new NopCommerceC5ConnectorSettings()
            {
                ApiKey = "",
                DefaultListId = "",
                WebHookKey = "",
            };
            _settingService.SaveSetting(settings);


            //locales
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.ApiKey", "MailChimp API Key");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.DefaultListId", "Default MailChimp List");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.AutoSync", "Use AutoSync task");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.AutoSyncEachMinutes", "AutoSync task period (minutes)");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.AutoSyncRestart", "If sync task period has been changed, please restart the application");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.WebHookKey", "WebHooks Key");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.QueueAll", "Initial Queue");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.QueueAll.Hint", "Queue existing newsletter subscribers (run only once)");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.ManualSync", "Manual Sync");
            plugin.AddOrUpdatePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.ManualSync.Hint", "Manually synchronize nopCommerce newsletter subscribers with MailChimp database");

            //Install sync task
            InstallSyncTask();

            //Install the database tables
            _trackingObjectContext.Install();
        }

        /// <summary>
        /// Uninstalls this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Uninstall(BasePlugin plugin)
        {
            //settings
            _settingService.DeleteSetting<NopCommerceC5ConnectorSettings>();

            //locales
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.ApiKey");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.DefaultListId");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.AutoSync");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.AutoSyncEachMinutes");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.AutoSyncRestart");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.WebHookKey");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.QueueAll");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.QueueAll.Hint");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.ManualSync");
            plugin.DeletePluginLocaleResource("Nop.Plugin.Other.NopCommerceC5Connector.ManualSync.Hint");

            //Remove scheduled task
            var task = FindScheduledTask();
            if (task != null)
                _scheduleTaskService.DeleteTask(task);

            //Uninstall the database tables
            _trackingObjectContext.Uninstall();
        }
    }
}
