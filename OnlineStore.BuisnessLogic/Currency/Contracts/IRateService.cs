using System.Globalization;

namespace OnlineStore.BuisnessLogic.Currency.Contracts
{
    public interface IRateService
    {
        decimal GetRate(CultureInfo cultureFrom, CultureInfo cultureTo);
    }
}