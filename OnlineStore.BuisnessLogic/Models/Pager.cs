﻿namespace OnlineStore.BuisnessLogic.Models
{
    public class Pager
    {
        public int PageIndex { get; set; }

        public int PagesCount { get; set; }

        public int PageSize { get; set; }

        public int[] Pages { get; set; }

        public PagerSettings PagerSettings { get; set; }
    }
}