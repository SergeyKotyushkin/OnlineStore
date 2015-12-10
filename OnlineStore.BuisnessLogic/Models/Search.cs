using System.Collections.Generic;
using System.Web.Mvc;

namespace OnlineStore.BuisnessLogic.Models
{
    public class Search
    {
        public string Name { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }

        public int SelectedCategory { get; set; }
    }
}