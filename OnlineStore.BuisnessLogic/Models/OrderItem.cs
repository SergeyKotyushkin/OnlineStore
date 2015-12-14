namespace OnlineStore.BuisnessLogic.Models
{
    public class OrderItem
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
    }
}