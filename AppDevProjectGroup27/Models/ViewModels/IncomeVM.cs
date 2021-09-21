using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class IncomeVM
    {
        public IEnumerable<ItemVM> IncomeList { get; set; }

        public double IncomeMade = 0.0;

        public int ItemSoldDay = 0;

        public DateTime DisplayDate { get; set; }

        public DateTime EarliestDay { get; set; }

        public DateTime CurrentDate { get; set; }
    }
}
