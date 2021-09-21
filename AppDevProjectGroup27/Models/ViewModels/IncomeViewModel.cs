using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class IncomeViewModel
    {
        public List<string> Name { get; set; }
        public List<double> Price { get; set; }
        public List<int> Quantity { get; set; }

        public double IncomeMade = 0.0;

        public int ItemSoldDay = 0;
    }
}
