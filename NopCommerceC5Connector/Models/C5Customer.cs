using Nop.Core;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Models
{
    public class C5Customer : IC5Entity
    {
        public string C5Number { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PostNumber { get; set; }
        public string FirstName { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string DebitorGroup { get; set; }
        public string Language { get; set; }
        public string Email { get; set; }
        public string PaymentEmail { get; set; }
        public string Currency { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        private List<C5Customer> Entities { get; set; }
    }
}