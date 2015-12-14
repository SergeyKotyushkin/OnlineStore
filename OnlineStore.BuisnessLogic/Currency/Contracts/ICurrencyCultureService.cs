using System.Globalization;

namespace OnlineStore.BuisnessLogic.Currency.Contracts
{
    public interface ICurrencyCultureService<in T>
    {
        CultureInfo GetCurrencyCultureInfo(T repository, string cultureNameInRepository);
    }
}