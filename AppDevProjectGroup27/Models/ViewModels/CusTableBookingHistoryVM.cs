using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class CusTableBookingHistoryVM
    {
        public IList<TableBookingHeader> TableBookings { get; set; }
        public IList<SelectListItem> Status { get; set; }
        public IList<SelectListItem> BookStatus { get; set; }

        public DateTime EarliestDate { get; set; }

        public DateTime CurrentDate { get; set; }

        public DateTime? DisplayDate { get; set; }

        public string StatusChosen { get; set; }
        public string BookStatusChosen { get; set; }

    }
}
