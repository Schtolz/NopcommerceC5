using CsvHelper;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Other.NopCommerceC5Connector.Models;
using Nop.Plugin.Other.NopCommerceC5Connector.Services.ImportServices;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services
{
    public class DiscountImportService:ImportService
    {
        private ICustomerService _customerService;
        private ICategoryService _categoryService;
        private IEnumerable<CustomerRole> _customerRoles;
        private IEnumerable<Category> _categories;
        private string namePrefix = "__discount_";

        public DiscountImportService(
            ICustomerService customerService, 
            ICurrencyService currencyService, 
            IDiscountService discountService,
            ILanguageService languageService,
            ICountryService countryService,
            IGenericAttributeService genericAttributeService,
            ICategoryService categoryService
            )
            : base(discountService, currencyService, languageService, genericAttributeService, countryService)
        {
            this._customerService = customerService;
            this._categoryService = categoryService;
            _customerRoles = _customerService.GetAllCustomerRoles(true);
            _categories = _categoryService.GetAllCategories(true);
        }

        public override void Import(System.Web.HttpPostedFileBase file)
        {
            base.Import(file);
            var discounts = LoadDataFromSource(file);
            PrepareCustomerRoles(discounts);
            EnterDiscounts(discounts);
        }

        private void PrepareCustomerRoles(List<C5Discount> discounts)
        {
            var customerRoles = _customerService.GetAllCustomerRoles(true);

            foreach (var roleToDelete in customerRoles.Where(x => x.Name.StartsWith(namePrefix)))
            {
                _customerService.DeleteCustomerRole(roleToDelete);
            }

            foreach (var c5Discount in discounts)
            {
                var roleName = GetDiscountName(c5Discount);
                if (!customerRoles.Any(x => x.Name == roleName))
                {
                    _customerService.InsertCustomerRole(
                    new CustomerRole()
                    {
                        Active = true,
                        Name = roleName,
                        IsSystemRole = false,
                        SystemName = roleName
                    });
                }
            }

            _customerRoles = _customerService.GetAllCustomerRoles(true).Where(x => x.Name.StartsWith(namePrefix));
        }

        private string GetDiscountName(C5Discount c5Discount)
        {
            return namePrefix + c5Discount.CategoryName + "_" + c5Discount.CustomerId;
        }

        private void EnterDiscounts(List<C5Discount> discounts)
        {
            foreach (var c5Discount in discounts.Where(x => x.Type == C5Discount.C5DiscountType.Gruppe))
            {
                var customer = _customerService.GetCustomerBySystemName(c5Discount.CustomerId);
                if (customer == null)
                    continue;
                var discountName = GetDiscountName(c5Discount);
                if (_discounts.Any(x => x.Name == discountName))
                    _discountService.DeleteDiscount(_discounts.First(x => x.Name == discountName));
                var discount = new Discount()
                {
                    Name = discountName,
                    DiscountAmount = c5Discount.Value,
                    DiscountType = DiscountType.AssignedToCategories,
                    UsePercentage = true
                };

                var discountRole = _customerRoles.First(x => x.Name == discountName);

                discount.DiscountRequirements.Add(new DiscountRequirement()
                    {
                         RestrictedToCustomerRoleId = discountRole.Id
                    });
             
                _discountService.InsertDiscount(discount);

                customer.CustomerRoles.Add(discountRole);
                _customerService.UpdateCustomer(customer);

                if (_categories.Any(x => x.Name.ToLower() == c5Discount.CategoryName.ToLower()))
                {
                    _categoryService.UpdateHasDiscountsApplied(_categories.First(x => x.Name.ToLower() == c5Discount.CategoryName.ToLower()));
                }
            }
        }

        public List<C5Discount> LoadDataFromSource(System.Web.HttpPostedFileBase file)
        {
            List<C5Discount> c5Discounts = new List<C5Discount>();
            if (file.ContentType.Contains("csv"))
            {
                using (var reader = new StreamReader(file.InputStream))
                {
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration() { Delimiter = ';', Quote = '"', HasHeaderRecord = false, SkipEmptyRecords = false }))
                    {
                        c5Discounts = csv.GetRecords<C5Discount>().ToList();
                    }
                }
            }
            else
            {
                using (var xlPackage = new ExcelPackage(file.InputStream))
                {
                    var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                        throw new NopException("No worksheet found");
                    c5Discounts = LoadFromExcelSheet(worksheet);
                }
            }
            return c5Discounts;
        }

        public List<C5Discount> LoadFromExcelSheet(ExcelWorksheet worksheet)
        {
            var c5Discounts = new List<C5Discount>();
            var properties = new string[]
                {
                    "Varekode",
                    "Target",
                    "Kontokode",
                    "CustomerId",
                    "Fra dato",
                    "Til dato",
                    "Antal",
                    "Sats",
                    "Valuta",
                    "Samme",
                    "Sog"
                };

            int iRow = 5;
            while (true)
            {
                bool allColumnsAreEmpty = true;
                for (var i = 1; i <= properties.Length; i++)
                    if (worksheet.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(worksheet.Cells[iRow, i].Value.ToString()))
                    {
                        allColumnsAreEmpty = false;
                        break;
                    }
                if (allColumnsAreEmpty)
                    break;

                string name = worksheet.Cells[iRow, GetColumnIndex(properties, "Navn")].Value as string;
                if (string.IsNullOrEmpty(name))
                {
                    iRow += 1;
                    continue;
                }
                C5Discount.C5DiscountType type = C5Discount.C5DiscountType.Specific;
                Enum.TryParse((string)worksheet.Cells[iRow, GetColumnIndex(properties, "Konto")].Value, out type);
                string target = worksheet.Cells[iRow, GetColumnIndex(properties, "Target")].Value as string;
                string customerId = worksheet.Cells[iRow, GetColumnIndex(properties, "CustomerId")].Value as string;
                decimal value = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Sats")].Value);

                c5Discounts.Add(new C5Discount()
                {
                    Type = type,
                    CategoryName = type == C5Discount.C5DiscountType.Gruppe ? target : null,
                    ProductName = type == C5Discount.C5DiscountType.Specific ? target : null,
                    CustomerId = customerId,
                    Value = value
                });
                iRow += 1;
            }

            return c5Discounts;
        }
    }
}
