using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class TableBookingHistVM
    {
        public IEnumerable<TableBookingHeader> tableBookingHeaders { get; set; }
        public PagingInfo PagingInfo { get; set; }
    }
}
