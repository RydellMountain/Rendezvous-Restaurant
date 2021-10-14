using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class CustomerBookingVM
    {
        public TableTrack TableTracks { get; set; }

        public IEnumerable<SelectListItem> TableList { get; set; }

        public int ChosenTableId { get; set; }

        public DateTime? DateChosen { get; set; }

        public DateTime? TimeChosen { get; set; }

        public int Quantity { get; set; }
    }
}
