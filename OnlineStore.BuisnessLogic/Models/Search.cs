using System.Collections.Generic;
using System.Web.Mvc;

namespace OnlineStore.BuisnessLogic.Models
{
    public class Search
    {
        public string SearchName { get; set; }

        public string SearchCategory { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}