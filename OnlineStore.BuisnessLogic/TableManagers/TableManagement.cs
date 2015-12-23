using System;
using System.Globalization;
using OnlineStore.BuisnessLogic.Currency.Contracts;
using OnlineStore.BuisnessLogic.Database.Contracts;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.TableManagers.Contracts;

namespace OnlineStore.BuisnessLogic.TableManagers
{
    public class TableManagement : ITableManagement
    {
        private readonly ICurrencyConverter _currencyConverter;
        private readonly IDbProductRepository _dbProductRepository;

        public TableManagement(ICurrencyConverter currencyConverter, IDbProductRepository dbProductRepository)
        {
            _currencyConverter = currencyConverter;
            _dbProductRepository = dbProductRepository;
        }

        public EditingResults AddOrUpdateProduct(string idString, string name, string category, string priceString,
            CultureInfo currencyCulture)
        {
            var id = int.Parse(idString ?? "-1");
            decimal price;

            return CheckIsNewProductValid(name, category, priceString, out price, currencyCulture)
                ? SetProduct(new Product { Id = id, Name = name, Category = category, Price = price }, currencyCulture)
                : EditingResults.FailValidProduct;
        }


        private static bool CheckIsNewProductValid(string name, string category, string price, out decimal priceResult,
            IFormatProvider currencyCulture)
        {
            var rgx = new System.Text.RegularExpressions.Regex(@"^[a-zA-Z]+[a-zA-Z0-9_ ]*$");

            var isDeimal = decimal.TryParse(price, NumberStyles.Currency, currencyCulture, out priceResult);

            return isDeimal && rgx.IsMatch(name) && rgx.IsMatch(category);
        }

        private EditingResults SetProduct(Product product, CultureInfo currencyCulture)
        {
            product.Price = _currencyConverter.ConvertToRubles(currencyCulture, product.Price, DateTime.Now);

            return _dbProductRepository.AddOrUpdate(product) ? EditingResults.Success : EditingResults.FailAddOrUpdate;
        }
    }
}