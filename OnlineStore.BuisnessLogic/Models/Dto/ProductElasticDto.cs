namespace OnlineStore.BuisnessLogic.Models.Dto
{
    public class ProductElasticDto
    {
        public ProductElasticDto(int id, string name, string category)
        {
            Id = id;
            Name = name;
            Category = category;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }
    }
}