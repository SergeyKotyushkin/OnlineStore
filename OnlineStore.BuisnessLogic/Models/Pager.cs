namespace OnlineStore.BuisnessLogic.Models
{
    public class Pager
    {
        public int PageIndex { get; set; }

        public int PagesCount { get; set; }

        public int PageSize { get; set; }
        
        public bool PagerVisible { get; set; }

        public int[] Pages { get; set; }
    }
}