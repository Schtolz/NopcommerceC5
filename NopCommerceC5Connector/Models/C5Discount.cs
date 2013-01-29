using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Models
{
    public class C5Discount
    {
        public C5DiscountType Type { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public string CustomerId { get; set; }
        public decimal Value { get; set; }
       
        public enum C5DiscountType
        {
            Specific, Gruppe
        }
    }
}