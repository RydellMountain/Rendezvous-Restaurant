using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class TableARVM
    {
        public List<TableBookingHeader> tableBookingHeaders { get; set; }
        public string StatusMessage = "";
    }
}
