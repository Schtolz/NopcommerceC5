using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Plugin.Other.NopCommerceC5Connector.Services.ImportServices
{
    public class ImportService:IImportService
    {
        protected IEnumerable<Discount> _discounts = null;
        protected IEnumerable<Currency> _currencies = null;
        protected IEnumerable<Language> _languages = null;
        protected IEnumerable<Country> _countries = null;

        protected readonly ICurrencyService _currencyService;
        protected readonly IGenericAttributeService _genericAttributeService;
        protected readonly IDiscountService _discountService;
        protected readonly ILanguageService _languageService;
        protected readonly ICountryService _countryService;

        protected static int GetColumnIndex(string[] properties, string columnName)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");

            if (columnName == null)
                throw new ArgumentNullException("columnName");

            for (int i = 0; i < properties.Length; i++)
                if (properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                    return i + 1; //excel indexes start from 1
            return 0;
        }

        public ImportService(
            IDiscountService discountService, 
            ICurrencyService currencyService, 
            ILanguageService languageService,
            IGenericAttributeService genericAttributeService, 
            ICountryService countryService
            )
        {
            this._currencyService = currencyService;
            this._discountService = discountService;
            this._languageService = languageService;
            this._countryService = countryService;
            this._genericAttributeService = genericAttributeService;
        }

        public virtual void Import(HttpPostedFileBase file)
        {
            _currencies = _currencyService.GetAllCurrencies();
            _discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, true);
            _languages = _languageService.GetAllLanguages(true);
            _countries = _countryService.GetAllCountries(true);
        }
    }
}