namespace OnlineStore.BuisnessLogic.Database.Models.Dto
{
    public class ProductDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public string Price { get; set; }

        public int OrderCount { get; set; }
    }
}