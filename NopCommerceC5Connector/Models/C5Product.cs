using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Models
{
    public class C5Product:IC5Entity
    {
        [CsvField(Index = 0)]
        public string ProductSku { get; set; }
        [CsvField(Index = 1)]
        public string ProductName { get; set; }
        [CsvField(Index = 2)]
        public string ShortDescription { get; set; }
        [CsvField(Index = 3)]
        public string LongDescription { get; set; }
        [CsvField(Index = 4)]
        public decimal Price { get; set; }
        [CsvField(Index = 5)]
        public int Stock { get; set; }
        [CsvField(Index = 6)]
        public decimal Weight { get; set; }
        [CsvField(Index = 7)]
        public string Image { get; set; }
        [CsvField(Index = 8)]
        public string CategoryCode { get; set; }
        [CsvField(Index = 9)]
        public string CategoryName { get; set; }
        [CsvField(Index = 10)]
        public string DiscountGroup { get; set; }
        [CsvField(Index = 11)]
        public int MinimumQuantity { get; set; }
        [CsvField(Index = 12)]
        public string Currency { get; set; }
    }
}