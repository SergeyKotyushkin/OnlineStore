﻿using System;
using System.Globalization;
using System.Linq;
using OnlineStore.BuisnessLogic.Models;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.MappingDto
{
    public static class OrderHistoryItemDtoMapping
    {
        public static OrderHistoryItemDto ToDto(OrderHistoryItem source, IFormatProvider culture, string quantityTitle, string priceTitle, string totalTitle)
        {
            CultureInfo currencyCulture;
            try
            {
                currencyCulture = CultureInfo.GetCultureInfo(source.CultureName);
            }
            catch
            {
                return null;
            }

            const string newLine = "<br/>";
            var order = source.ProductOrder.Aggregate(string.Empty,
                (current, productOrder) =>
                    current +
                    string.Format("<b>{0}</b> ({1} <b>{2}</b>: {3} <b>{4}</b>)" + " {5} <b>{6}</b>{7}",
                        productOrder.Name, quantityTitle, productOrder.Count, priceTitle,
                        productOrder.Price, totalTitle,
                        productOrder.Total, newLine));

            return new OrderHistoryItemDto
            {
                Number = source.Number,
                Date = source.Date.ToString(culture),
                Email = source.Email,
                Order = order.Substring(0, order.Length - newLine.Length),
                Total = string.Format("<b>{0}</b>", source.Total.ToString("C", currencyCulture))
            };
        } 
    }
}