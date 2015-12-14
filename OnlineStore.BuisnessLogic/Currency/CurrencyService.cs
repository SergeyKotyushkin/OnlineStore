using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OnlineStore.BuisnessLogic.Currency.Contracts;

namespace OnlineStore.BuisnessLogic.Currency
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IRateService _rateService;

        private readonly List<RateScheme> _rateSchemes = new List<RateScheme>();

        private readonly TimeSpan _updatedPeriod = new TimeSpan(0, 0, 30, 0);  // 30 minutes

        public CurrencyService(IRateService rateService)
        {
            _rateService = rateService;
        }


        public bool CheckIsRateActual(CultureInfo cultureForm, CultureInfo cultureTo, DateTime dateTimeNow)
        {
            var rateScheme = GetRateScheme(cultureForm, cultureTo);
            if (rateScheme == null) return false; // null if this rate scheme is apsent

            return dateTimeNow - rateScheme.Updated < _updatedPeriod;
        }

        public decimal GetRealTimeRate(CultureInfo cultureForm, CultureInfo cultureTo, DateTime dateTimeNow)
        {
            var rate = _rateService.GetRate(cultureForm, cultureTo);

            SetRate(cultureForm, cultureTo, dateTimeNow, rate);

            return rate;
        }

        public decimal GetRate(CultureInfo cultureForm, CultureInfo cultureTo, DateTime dateTimeNow)
        {
            var rateScheme = GetRateScheme(cultureForm, cultureTo);

            return rateScheme != null
                ? (Equals(rateScheme.From, cultureForm) ? rateScheme.RateDirect : rateScheme.RateInverse)
                : GetRealTimeRate(cultureForm, cultureTo, dateTimeNow);
        }


        private RateScheme GetRateScheme(CultureInfo cultureForm, CultureInfo cultureTo)
        {
            var rateScheme =
                _rateSchemes.FirstOrDefault(r => (Equals(r.From, cultureForm) && Equals(r.To, cultureTo)) ||
                                                 (Equals(r.From, cultureTo) && Equals(r.To, cultureForm)));

            return rateScheme;
        }

        private void SetRate(CultureInfo cultureForm, CultureInfo cultureTo, DateTime dateTimeNow, decimal rate)
        {
            var rateScheme = GetRateScheme(cultureForm, cultureTo);

            var inverseRate = 1/rate;

            if (rateScheme == null)
                _rateSchemes.Add(new RateScheme
                {
                    From = cultureForm,
                    To = cultureTo,
                    RateDirect = rate,
                    RateInverse = inverseRate,
                    Updated = dateTimeNow
                });
            else
            {
                rateScheme.RateDirect = Equals(rateScheme.From, cultureForm) ? rate : inverseRate;
                rateScheme.RateInverse = Equals(rateScheme.From, cultureForm) ? inverseRate : rate;
                rateScheme.Updated = dateTimeNow;
            }
        }
    }
}