using Nop.Core.Data;
using Nop.Plugin.Other.NopCommerceC5Connector.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services
{
    public class ViewTrackingService : IViewTrackingService
    {
        private readonly IRepository<TrackingRecord> _trackingRecordRepository;

        public ViewTrackingService(IRepository<TrackingRecord> trackingRecordRepository)
        {
            _trackingRecordRepository = trackingRecordRepository;
        }

        /// <summary>
        /// Logs the specified record.
        /// </summary>
        /// <param name="record">The record.</param>
        public void Log(TrackingRecord record)
        {
            _trackingRecordRepository.Insert(record);
        }
    }
}
