namespace OnlineStore.BuisnessLogic.Models
{
    public class Table<TData>
    {
        public TData[] Data { get; set; }

        public Pager Pager { get; set; }
    }
}