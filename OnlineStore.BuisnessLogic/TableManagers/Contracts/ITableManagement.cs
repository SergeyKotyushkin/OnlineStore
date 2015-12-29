using System.Globalization;
using OnlineStore.BuisnessLogic.Models;

namespace OnlineStore.BuisnessLogic.TableManagers.Contracts
{
    public interface ITableManagement
    {
        EditProductResult AddOrUpdateProduct(string idString, string name, string category, string priceString,
            CultureInfo currencyCulture);
    }
}