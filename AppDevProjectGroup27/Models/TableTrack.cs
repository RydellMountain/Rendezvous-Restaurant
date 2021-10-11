using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models
{
    public class TableTrack
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TableId { get; set; }

        [ForeignKey("TableId")]
        public virtual Table Table { get; set; }

        [Display(Name = "Amount Available")]
        [Required]
        public int AmtAva { get; set; }

        [Display(Name = "Date")]
        [Required]
        public DateTime DateTable { get; set; }

        [Display(Name = "Time")]
        [Required]
        public TimeSpan TimeTable { get; set; }
    }
}
