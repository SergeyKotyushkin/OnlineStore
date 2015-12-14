using System;
using System.Globalization;
using System.Linq;

namespace OnlineStore.BuisnessLogic.Models
{
    public class OrderToGrid
    {
        public OrderToGrid(OrderFromHistory source, string quantityTitle, string priceTitle, string totalTitle)
        {
            Number = source.Number;
            Date = source.Date;
            Email = source.Email;
            var culture =
                CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(c => c.Name == source.CultureName);

            if (culture == null)
            {
                Order = "Sorry, unknown currency.";
                return;
            }

            var order = source.ProductsOrder.Aggregate(string.Empty,
                (current, p) =>
                    current +
                    string.Format("<b>{0}</b> ({1} <b>{2}</b>: {3} <b>{4}</b>)" + " {5} <b>{6}</b>{7}", p.Name,
                        quantityTitle, p.Count, priceTitle, p.Price.ToString("C", culture), totalTitle,
                        p.Total.ToString("C", culture), Environment.NewLine));

            Order = order.Substring(0, order.Length - Environment.NewLine.Length);
            Total = string.Format("<b>{0}</b>", source.Total.ToString("C", culture));
        }

        public int Number { get; set; }
        public DateTime Date { get; set; }
        public string Email { get; set; }
        public string Order { get; set; }
        public string Total { get; set; }
    }
}