﻿namespace OnlineStore.BuisnessLogic.Models
{
    public class MainLayoutSettings
    {
        public string Title { get; set; }

        public bool MoneyVisible { get; set; }

        public bool ProfileVisible { get; set; }

        public bool LogoutVisible { get; set; }

        public string SelectedLanguage { get; set; }

        public string SelectedCurrency { get; set; }

        public Route RouteBack { get; set; }
    }
}