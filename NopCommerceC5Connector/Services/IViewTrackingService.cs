using Nop.Plugin.Other.NopCommerceC5Connector.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services
{
    public interface IViewTrackingService
    {
        /// <summary>
        /// Logs the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        void Log(TrackingRecord record);
    }
}
