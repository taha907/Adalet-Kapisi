using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BuroManagementProject.Models
{
    public class DavaEkleViewModel
    {
        public Dava Dava { get; set; } = new Dava();
        public List<int> SecilenMuvekkilIdListesi { get; set; } = new List<int>();
        public List<SelectListItem> Muvekkiller { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Mahkemeler { get; set; } = new List<SelectListItem>();
    }
}
