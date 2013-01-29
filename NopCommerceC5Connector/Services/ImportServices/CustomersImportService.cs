using CsvHelper;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Other.NopCommerceC5Connector.Models;
using Nop.Plugin.Other.NopCommerceC5Connector.Services.ImportServices;
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
    public class CustomersImportService:ImportService
    {
        private readonly ICustomerService _customerService;
        private IEnumerable<CustomerRole> _customerRoles;

        public CustomersImportService(
            ICustomerService customerService, 
            ICurrencyService currencyService, 
            IDiscountService discountService,
            ILanguageService languageService,
            ICountryService countryService,
            IGenericAttributeService genericAttributeService
            )
            : base(discountService, currencyService, languageService, genericAttributeService, countryService)
        {
            this._customerService = customerService;
        }
        
        public override void Import(System.Web.HttpPostedFileBase file)
        {
            base.Import(file);
            var customers = LoadDataFromSource(file);
            PrepareCurrencies(customers.GroupBy(x => x.Currency).Select(y=>y.Key));
            PrepareCustomerRoles(customers.GroupBy(x => x.DebitorGroup).Select(y=>y.Key));
            EnterCustomers(customers);
        }

        private void PrepareCurrencies(IEnumerable<string> c5currencies)
        {
            var currencies = _currencyService.GetAllCurrencies();
            foreach (var currencyName in c5currencies.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (!currencies.Any(x => x.Name == currencyName))
                {
                    _currencyService.InsertCurrency(
                        new Currency()
                        {
                            CurrencyCode = currencyName,
                            Name = currencyName,
                            Published = true,
                            CreatedOnUtc=DateTime.Now,
                            UpdatedOnUtc = DateTime.Now
                        }
                    );
                }
            }
            _currencies = _currencyService.GetAllCurrencies();
        }


        private void PrepareCustomerRoles(IEnumerable<string> rolesGroups)
        {
            var customerRoles = _customerService.GetAllCustomerRoles(true);
            foreach (var customerRole in rolesGroups.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (!customerRoles.Any(x => x.Name == customerRole))
                {
                    _customerService.InsertCustomerRole(
                    new CustomerRole()
                    {
                        Active = true,
                        Name = customerRole,
                        IsSystemRole = false,
                        SystemName = customerRole
                    });
                }
            }
            _customerRoles = _customerService.GetAllCustomerRoles(true);

            foreach(var cRole in rolesGroups)
            {
                var role = _customerRoles.First(x=>x.Name == cRole);
                foreach(var customer in _customerService.GetCustomersByCustomerRoleId(role.Id))
                {
                    _customerService.DeleteCustomer(customer);
                }
            }
        }

        private void EnterCustomers(IEnumerable<C5Customer> c5Customers)
        {
            foreach (var c5Customer in c5Customers)
            {
                var currency = _currencies.FirstOrDefault(x => x.Name == c5Customer.Currency);
                //var language = _languages.FirstOrDefault(x => x.Name == c5Customer.Language);
                var customer = new Customer()
                {
                    Active = true,
                    AdminComment = c5Customer.C5Number,
                    Currency = currency,
                    CurrencyId = currency.Id,
                    Email = c5Customer.Email,
                    //Language = language,
                    //LanguageId = language.Id,
                    Password = "Customer",
                    Username = c5Customer.Name,
                    CreatedOnUtc = DateTime.UtcNow,
                    LastActivityDateUtc = DateTime.UtcNow,
                    SystemName = c5Customer.C5Number
                };

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.FirstName, c5Customer.Name);

                //Adding C5 customer role
                if(_customerRoles.Any(x=>x.Name == c5Customer.DebitorGroup))
                {
                    customer.CustomerRoles.Add(_customerRoles.First(x=>x.Name == c5Customer.DebitorGroup));
                }

                //Adding Default roles
                customer.CustomerRoles.Add(_customerRoles.First(x => x.Name == "Registered"));
                AddCustomerAddress(c5Customer, customer);

                if (_customerService.GetCustomerByUsername(customer.Username)!=null)
                    _customerService.DeleteCustomer(_customerService.GetCustomerByUsername(customer.Username));

                _customerService.InsertCustomer(customer);
            }
        }

        private void AddCustomerAddress(C5Customer c5Customer, Customer customer)
        {
            var coutry = _countries.Any(x => x.Name == c5Customer.Country) ? _countries.First(x => x.Name == c5Customer.Country) : _countries.First(x => x.Name == "Denmark");
            var address = new Address()
            {
                Address1 = c5Customer.Address1,
                Address2 = c5Customer.Address2,
                City = c5Customer.City,
                Company = c5Customer.Name,
                Country = coutry,
                CountryId = coutry.Id,
                Email = c5Customer.Email,
                FaxNumber = c5Customer.Fax,
                FirstName = c5Customer.FirstName,
                PhoneNumber = c5Customer.Phone,
                ZipPostalCode = c5Customer.PostNumber,
                CreatedOnUtc = DateTime.Now
            };
            customer.Addresses.Add(address);
        }


        public List<C5Customer> LoadDataFromSource(System.Web.HttpPostedFileBase file)
        {
            List<C5Customer> c5Products = new List<C5Customer>();
            if (file.ContentType.Contains("csv"))
            {
                using (var reader = new StreamReader(file.InputStream))
                {
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration() { Delimiter = ';', Quote = '"', HasHeaderRecord = false, SkipEmptyRecords = false }))
                    {
                        c5Products = csv.GetRecords<C5Customer>().ToList();
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
                    c5Products = LoadFromExcelSheet(worksheet);
                }
            }
            return c5Products;
        }

        public List<C5Customer> LoadFromExcelSheet(ExcelWorksheet worksheet)
        {
            var c5Customers = new List<C5Customer>();
            var properties = new string[]
                {
                    "Konto",
                    "Navn",
                    "Adresse",
                    "Adresse (1)",
                    "Postnr/by",
                    "Land",
                    "Attention",
                    "Telefon",
                    "Telefax",
                    "Fakturakonto",
                    "Gruppe",
                    "Fast rabat",
                    "Godkendt",
                    "Prisgruppe",
                    "Rabatgruppe",
                    "Kasserabat",
                    "Billede",
                    "Valuta",
                    "Sprog",
                    "Betaling",
                    "Levering",
                    "Spærret",
                    "Sælger",
                    "Moms",
                    "Gironummer",
                    "Momsnummer",
                    "Rente",
                    "Afdeling",
                    "Max. rykker",
                    "Engangskunde",
                    "Beholdning",
                    "EDI-adresse",
                    "Saldo",
                    "1-30 dage",
                    "31-60 dage",
                    "61-90 dage",
                    "91-120 dage",
                    "Over 120 dage",
                    "Forfalden",
                    "Beregnet",
                    "Højeste saldo",
                    "Saldo DKK",
                    "Søgenavn",
                    "Kontant",
                    "Indbetalmåde",
                    "Ordregrp.",
                    "Projektgrp.",
                    "Handel",
                    "Transport",
                    "E-mail",
                    "Hjemmeside",
                    "Mobil",
                    "Kraknr",
                    "Bærer",
                    "Formål",
                    "EAN-nummer",
                    "Kontodim",
                    "OIOXML",
                    "Fakturadato",
                    "Betaling (1)",
                    "Rykker",
                    "Rente (1)",
                    "Faktura",
                    "XML import",
                    "E-mail fakturering",
                    "Afgift",
                };

            int iRow = 2;
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
                string c5Number = worksheet.Cells[iRow, GetColumnIndex(properties, "Konto")].Value as string;
                string address1 = worksheet.Cells[iRow, GetColumnIndex(properties, "Adresse")].Value as string;
                string address2 = worksheet.Cells[iRow, GetColumnIndex(properties, "Adresse (1)")].Value as string;
                string attention = worksheet.Cells[iRow, GetColumnIndex(properties, "Attention")].Value as string;
                string email = worksheet.Cells[iRow, GetColumnIndex(properties, "E-mail")].Value as string;
                string paymentEmail = worksheet.Cells[iRow, GetColumnIndex(properties, "E-mail fakturering")].Value as string;
                string fax = worksheet.Cells[iRow, GetColumnIndex(properties, "Telefax")].Value as string;
                string language = worksheet.Cells[iRow, GetColumnIndex(properties, "Sprog")].Value as string;
                string phone = worksheet.Cells[iRow, GetColumnIndex(properties, "Telefon")].Value as string;
                string postNumberCity = worksheet.Cells[iRow, GetColumnIndex(properties, "Postnr/by")].Value as string;
                string group = worksheet.Cells[iRow, GetColumnIndex(properties, "Gruppe")].Value as string;
                string country = worksheet.Cells[iRow, GetColumnIndex(properties, "Land")].Value as string;
                string currency = worksheet.Cells[iRow, GetColumnIndex(properties, "Valuta")].Value as string;
                string city = postNumberCity;
                int postNumber = 0;
                if (postNumberCity != null && postNumberCity.IndexOf(' ')>=0)
                {
                    var expectedCityName = postNumberCity.Substring(postNumberCity.IndexOf(' '));
                    var expectedPostNumber = postNumberCity.Substring(0, postNumberCity.IndexOf(' '));
                    if (Int32.TryParse(expectedPostNumber, out postNumber))
                    {
                        city = expectedCityName;
                    }
                }

                c5Customers.Add(new C5Customer()
                {
                    C5Number = c5Number,
                    FirstName = attention,
                    Name = name,
                    Address1 = address1,
                    Address2 = address2,
                    Email = email,
                    PaymentEmail = paymentEmail,
                    Fax = fax,
                    Language = language,
                    Phone = phone,
                    PostNumber = postNumber == 0 ? postNumberCity : postNumber.ToString(),
                    City = city,
                    DebitorGroup = group,
                    Currency = currency,
                    Country = country
                });
                iRow += 1;
            }

            return c5Customers;
        }
    }
}
