using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Seater")]
        public string SeatingName { get; set; }

        [Display(Name = "Amount of Tables")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter an amount greater than 0.")]
        public int MaxTables { get; set; }

        public bool Active { get; set; }
    }
}
