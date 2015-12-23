using System.Linq;
using OnlineStore.BuisnessLogic.Database.Models;
using OnlineStore.BuisnessLogic.Searching.Contracts;

namespace OnlineStore.BuisnessLogic.Searching
{
    public class SimpleSearching : ISearching<Product>
    {
        public Product[] Search(Product[] data, string name, string category)
        {
            var result = data.Select(d => d);

            if (!string.IsNullOrEmpty(name))
                result = data.Where(p => p.Name.ToLower().Contains(name.ToLower()));

            if (!string.IsNullOrEmpty(category))
                result = result.Where(p => p.Category.ToLower().Contains(category.ToLower()));

            return result.ToArray();
        }
    }
}