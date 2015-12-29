using OnlineStore.BuisnessLogic.Database.Models;

namespace OnlineStore.BuisnessLogic.Models
{
    public class EditProductResult
    {
        public Product Product { get; set; }

        public EditingResults EditingResult { get; set; }
    }
}