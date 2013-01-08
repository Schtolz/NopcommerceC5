using Nop.Web.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Models
{

    public class NopCommerceC5ConnectorSettingsModel
    {
        private IList<SelectListItem> _listOptions;

        public virtual SelectList ImportTypes { get; set; }

        public virtual Utility.Utility.ImportType ImportType { get; set; }

        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        [NopResourceDisplayName("Plugin.Other.NopCommerceC5Connector.ApiKey")]
        public virtual string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the web hook key.
        /// </summary>
        /// <value>
        /// The web hook key.
        /// </value>
        [NopResourceDisplayName("Plugin.Other.NopCommerceC5Connector.WebHookKey")]
        public virtual string WebHookKey { get; set; }

        /// <summary>
        /// Gets or sets the default list id.
        /// </summary>
        /// <value>
        /// The default list id.
        /// </value>
        [NopResourceDisplayName("Plugin.Other.NopCommerceC5Connector.DefaultListId")]
        public virtual string DefaultListId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [auto sync].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [auto sync]; otherwise, <c>false</c>.
        /// </value>
        [NopResourceDisplayName("Plugin.Other.NopCommerceC5Connector.AutoSync")]
        public virtual bool AutoSync { get; set; }

        [NopResourceDisplayName("Plugin.Other.NopCommerceC5Connector.AutoSyncEachMinutes")]
        public virtual int AutoSyncEachMinutes { get; set; }

        /// <summary>
        /// Gets or sets the list options.
        /// </summary>
        /// <value>
        /// The list options.
        /// </value>
        public virtual IList<SelectListItem> ListOptions
        {
            get { return _listOptions ?? (_listOptions = new List<SelectListItem>()); }
            set { _listOptions = value; }
        }

        public string SaveResult { get; set; }

        public string SyncResult { get; set; }
    }
}
