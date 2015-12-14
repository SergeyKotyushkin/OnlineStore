using System;
using System.Configuration;
using System.Globalization;
using System.Net;
using System.Text;
using OnlineStore.BuisnessLogic.Currency.Contracts;

namespace OnlineStore.BuisnessLogic.Currency
{
    public class YahooRateService : IRateService
    {
        private const string ConnectingStringName = "YahooRateService";

        public decimal GetRate(CultureInfo cultureFrom, CultureInfo cultureTo)
        {
            var currencyFromSymbol = new RegionInfo(cultureFrom.LCID).ISOCurrencySymbol;
            var currencyToSymbol = new RegionInfo(cultureTo.LCID).ISOCurrencySymbol;
            var url =
                string.Format(
                    ConfigurationManager.AppSettings[ConnectingStringName],
                    currencyFromSymbol,
                    currencyToSymbol);

            var rateData = GetRateData(url);

            return ParseRateData(rateData);
        }

        private static byte[] GetRateData(string url)
        {
            using (var wc = new WebClient())
            {
                return wc.DownloadData(url);
            }
        }

        private static decimal ParseRateData(byte[] data)
        {
            var rateString = Encoding.Default.GetString(data);
            rateString = rateString.Substring(0, rateString.Length - 1).Substring(rateString.IndexOf(',') + 1);

            if (rateString == "N/A")
                throw new CultureNotFoundException();

            decimal rate;
            if (!decimal.TryParse(rateString, NumberStyles.Any, new CultureInfo("en-US"), out rate))
                throw new NullReferenceException();

            return rate;
        }
    }
}