using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class SpecialVM
    {
        public IEnumerable<MenuItems> MenuItems { get; set; }
        public string Subject { get; set; }
        public string StatusMessage { get; set; }
    }
}
