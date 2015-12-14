using System;
using System.Globalization;

namespace OnlineStore.BuisnessLogic.Currency.Contracts
{
    public interface ICurrencyService
    {
        bool CheckIsRateActual(CultureInfo cultureForm, CultureInfo cultureTo, DateTime dateTimeNow);

        decimal GetRealTimeRate(CultureInfo cultureForm, CultureInfo cultureTo, DateTime dateTimeNow);

        decimal GetRate(CultureInfo cultureForm, CultureInfo cultureTo, DateTime dateTimeNow);
    }
}