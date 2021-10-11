using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models.ViewModels
{
    public class TableEditVM
    {
        public Table Table { get; set; }
        public string StatusMessage { get; set; }

        [Required]
        public int OldMaxValue { get; set; }
    }
}
