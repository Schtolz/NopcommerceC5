using Nop.Plugin.Other.NopCommerceC5Connector.Domain;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Data
{
    public class TrackingRecordMap : EntityTypeConfiguration<TrackingRecord>
    {
        public TrackingRecordMap()
        {
            ToTable("ProductViewTracking");

            //Map the primary key
            HasKey(m => m.Id);
            //Map the additional properties
            Property(m => m.ProductId);
            //Avoiding truncation/failure 
            //so we set the same max length used in the product tame
            Property(m => m.ProductName).HasMaxLength(400);
            Property(m => m.IpAddress);
            Property(m => m.CustomerId);
            Property(m => m.IsRegistered);
        }
    }
}
