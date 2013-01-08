using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector
{
    public class NopCommerceC5ConnectorSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        /// <value>
        /// The API key.
        /// </value>
        public virtual string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the web hook key.
        /// </summary>
        /// <value>
        /// The web hook key.
        /// </value>
        public virtual string WebHookKey { get; set; }

        /// <summary>
        /// Gets or sets the default list id.
        /// </summary>
        /// <value>
        /// The default list id.
        /// </value>
        public virtual string DefaultListId { get; set; }
    }
}
