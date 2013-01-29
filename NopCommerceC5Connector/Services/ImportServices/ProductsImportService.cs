using CsvHelper;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Other.NopCommerceC5Connector.Models;
using Nop.Plugin.Other.NopCommerceC5Connector.Services.ImportServices;
using Nop.Services.Catalog;
using Nop.Services.Common;
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
    public class ProductsImportService:ImportService
    {
        protected readonly IProductService _productService;
        protected readonly ICategoryService _categoryService;

        public ProductsImportService(
            IProductService productService, 
            ICategoryService categoryService, 
            IDiscountService discountService, 
            ICurrencyService currencyService,
            ILanguageService languageService,
             ICountryService countryService,
            IGenericAttributeService genericAttributeService
            )
            : base(discountService, currencyService, languageService, genericAttributeService, countryService)
        {
            this._productService = productService;
            this._categoryService = categoryService;
        }

        public override void Import(System.Web.HttpPostedFileBase file)
        {
            base.Import(file);
            var c5Products = LoadDataFromSource(file);
            PrepareDiscounts(c5Products);
            PrepareCurrencies(c5Products.GroupBy(x=>x.Currency));
            EnterProducts(c5Products);
        }

        private void PrepareCurrencies(IEnumerable<IGrouping<string, C5Product>> c5currencies)
        {
            var currencies = _currencyService.GetAllCurrencies();
            foreach (var currencyName in c5currencies.Where(x => !string.IsNullOrEmpty(x.Key)))
            {
                if (!currencies.Any(x => x.Name == currencyName.Key))
                {
                    _currencyService.InsertCurrency(
                        new Currency()
                        {
                            CurrencyCode = currencyName.Key,
                            Name = currencyName.Key,
                            Published = true,
                            CreatedOnUtc = DateTime.Now,
                            UpdatedOnUtc = DateTime.Now
                        }
                    );
                }
            }
            _currencies = _currencyService.GetAllCurrencies();
        }

        private void PrepareDiscounts(IEnumerable<C5Product> c5Products)
        {
            var discountNames = c5Products.GroupBy(x => x.DiscountGroup);
            var discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, true);
            foreach (var discountName in discountNames.Where(x=>!string.IsNullOrEmpty(x.Key)))
            {
                if (!discounts.Any(x => x.Name == discountName.Key))
                {
                    _discountService.InsertDiscount(new Discount() { 
                        Name= discountName.Key,
                        DiscountType= DiscountType.AssignedToSkus,
                        UsePercentage = true
                    });
                }
            }
            _discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, true);
        }

        private void EnterProducts(IEnumerable<C5Product> c5Products)
        {
            foreach (var c5Product in c5Products)
            {
                var nopProductVariant = _productService.GetProductVariantBySku(c5Product.ProductSku);
                Product nopProduct = null;
                if (nopProductVariant != null)
                {
                    nopProduct = nopProductVariant.Product;
                }
                else
                {
                    nopProduct = CreateNopProduct(c5Product);
                    _productService.InsertProduct(nopProduct);

                    nopProductVariant = CreateNopProductVariant(c5Product, nopProduct);
                    _productService.InsertProductVariant(nopProductVariant);
                }

                nopProductVariant.Price = c5Product.Price;
                nopProductVariant.Weight = c5Product.Weight;
                nopProductVariant.StockQuantity = c5Product.Stock;
                nopProductVariant.Sku = c5Product.ProductSku;
                nopProductVariant.IsShipEnabled = true;
                nopProductVariant.HasDiscountsApplied = true;
                nopProductVariant.OrderMinimumQuantity = c5Product.MinimumQuantity;
                nopProductVariant.Name = c5Product.ProductName;
                nopProductVariant.Description = c5Product.ShortDescription;
                
                if (_discounts.Any(x => x.Name == c5Product.DiscountGroup))
                {
                    nopProductVariant.AppliedDiscounts.Add(_discounts.First(x => x.Name == c5Product.DiscountGroup));
                }
               
                nopProductVariant.ManageInventoryMethod = ManageInventoryMethod.ManageStock;
                _productService.UpdateProductVariant(nopProductVariant);

                nopProduct.ShortDescription = c5Product.ShortDescription;
                nopProduct.FullDescription = c5Product.LongDescription;
                UpdateProductCategories(c5Product, nopProduct);
                nopProduct.MetaTitle = c5Product.ProductName;
                nopProduct.MetaDescription = c5Product.ShortDescription;

                _productService.UpdateProduct(nopProduct);
            }
        }

        private void UpdateProductCategories(C5Product c5Product, Product nopProduct)
        {
            var c5CategoryIds = GetC5ProductCategories(c5Product);
            var productCategoriesIds = nopProduct.ProductCategories.ToList();
            foreach (var productCategory in productCategoriesIds)
                _categoryService.DeleteProductCategory(productCategory);

            foreach (var c5Category in c5CategoryIds)
            {
                var productCategory = CreateProductCategory(nopProduct, c5Category);
                _categoryService.InsertProductCategory(productCategory);
            }
        }

        private ProductCategory CreateProductCategory(Product nopProduct, Category c5Category)
        {
            return new ProductCategory()
            {
                ProductId = nopProduct.Id,
                CategoryId = c5Category.Id, //use Category property (not CategoryId) because appropriate property is stored in it
                IsFeaturedProduct = false,
                DisplayOrder = 1
            };
        }

        private ICollection<Category> GetC5ProductCategories(C5Product c5Product)
        {
            var categories = new List<Category>();
            foreach (var categoryName in c5Product.CategoryName.Split(','))
            {
                var existingCategories = _categoryService.GetAllCategories(categoryName, true);
                if (existingCategories.Count > 1)
                {
                    foreach (var category in existingCategories)
                    {
                        _categoryService.DeleteCategory(category);
                    }
                }
                if (existingCategories.Count > 0)
                {
                    categories.Add(existingCategories.First());
                }
                else {
                    var category = new Category()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow,
                        Name = c5Product.CategoryName,
                        Published = true
                    };
                    _categoryService.InsertCategory(category);
                    categories.Add(category);
                }
            }
            return categories;
        }

        private ProductVariant CreateNopProductVariant(C5Product c5Product, Product nopProduct)
        {
            return new ProductVariant()
            {
                ProductId = nopProduct.Id,
                Published = true,
                DisplayOrder = 1,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
        }

        private Core.Domain.Catalog.Product CreateNopProduct(C5Product c5Product)
        {
            return new Product()
            {
                Name = c5Product.ProductName,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
        }


        public List<C5Product> LoadDataFromSource(System.Web.HttpPostedFileBase file)
        {
            List<C5Product> c5Products = new List<C5Product>();
            if (file.ContentType.Contains("csv"))
            {
                using (var reader = new StreamReader(file.InputStream))
                {
                    using (var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration() { Delimiter = ';', Quote = '"', HasHeaderRecord = false, SkipEmptyRecords = false }))
                    {
                        c5Products = csv.GetRecords<C5Product>().ToList();
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

        public static List<C5Product> LoadFromExcelSheet(ExcelWorksheet worksheet)
        {
            var c5Products = new List<C5Product>();
            var properties = new string[]
                {
                    "Varenummer",
                    "Varenavn",
                    "Supp. varenavn",
                    "Supp. varenavn (1)",
                    "Varetype",
                    "Rabatgruppe",
                    "Kostvaluta",
                    "Kostpris",
                    "Gruppe",
                    "Salgsmodel",
                    "Lagermodel",
                    "Købskvanti",
                    "Leverandør",
                    "Lev. varenr",
                    "Spærret",
                    "Alternativ",
                    "Alt. vare",
                    "Decimaler",
                    "Provision",
                    "Billede",
                    "Nettovægt",
                    "Rumfang",
                    "Toldposition",
                    "Enhed",
                    "Engangsvare",
                    "Art",
                    "Omkostning",
                    "Kostmodel",
                    "Hovedlager",
                    "Lokation",
                    "Købsmoms",
                    "Beholdning",
                };

            int iRow = 3;
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

                string name = worksheet.Cells[iRow, GetColumnIndex(properties, "Varenavn")].Value as string;
                if (string.IsNullOrEmpty(name))
                {
                    iRow += 1;
                    continue;
                }
                string shortDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "Supp. varenavn")].Value as string;
                string fullDescription = worksheet.Cells[iRow, GetColumnIndex(properties, "Supp. varenavn (1)")].Value as string;
                string discountGroup = worksheet.Cells[iRow, GetColumnIndex(properties, "Rabatgruppe")].Value as string;
                decimal price = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Kostpris")].Value);
                string sku = worksheet.Cells[iRow, GetColumnIndex(properties, "Varenummer")].Value as string;
                int stockQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "Beholdning")].Value);
                decimal weight = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, "Nettovægt")].Value);
                string categoryName = worksheet.Cells[iRow, GetColumnIndex(properties, "Gruppe")].Value as string;
                int minimumQuantity = Convert.ToInt32(worksheet.Cells[iRow, GetColumnIndex(properties, "Købskvanti")].Value);
                string currency = worksheet.Cells[iRow, GetColumnIndex(properties, "Kostvaluta")].Value as string;
                

                c5Products.Add(new C5Product()
                {
                    ProductName = name,
                    Price = price,
                    ProductSku = sku,
                    Stock = stockQuantity,
                    Weight = weight,
                    LongDescription = fullDescription,
                    CategoryName = categoryName,
                    DiscountGroup = discountGroup,
                    ShortDescription = shortDescription,
                    MinimumQuantity = minimumQuantity == 0 ? 1 : minimumQuantity,
                    Currency= currency
                });
                iRow += 1;
            }

            return c5Products;
        }
    }
}
