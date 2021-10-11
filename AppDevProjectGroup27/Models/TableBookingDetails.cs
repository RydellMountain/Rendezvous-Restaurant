using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models
{
    public class TableBookingDetails
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TableBookingHeaderId { get; set; }

        [ForeignKey("TableBookingHeaderId")]
        public virtual TableBookingHeader TableBookingHeader { get; set; }

        [Display(Name = "Table Name")]
        [Required]
        public string TableName { get; set; }

        [Display(Name = "Tables Booked")]
        [Required]
        public int TableBooked { get; set; }

    }
}
