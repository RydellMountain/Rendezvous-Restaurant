using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models
{
    public class TableBookingHeader
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime DateBookingMade { get; set; }

        [Display(Name = "Sit-In Date")]
        [Required]
        public DateTime SitInDate { get; set; }

        [Display(Name = "Sit-In Time")]
        [Required]
        public TimeSpan SitInTime { get; set; }

        [Display(Name = "Table Name")]
        [Required]
        public string TableName { get; set; }

        [Display(Name = "Tables Booked")]
        [Required]
        public int TableBooked { get; set; }

        public string Status { get; set; }

        [Display(Name = "Book Status")]
        public string BookStatus { get; set; }

        [Display(Name = "Approved By")]

        public string ApprovedBy { get; set; }
        
        [Display(Name = "Cancelled/Rejected By")]
        public string RejectedBy { get; set; }

        [Display(Name = "Date/Time Approved")]
        public DateTime? TimeApproved { get; set; }

        [Display(Name = "Date/Time Rejected/Cancelled")]
        public DateTime? TimeRejected { get; set; }

        [Display(Name = "Date/Time Sit-In")]
        public DateTime? TimeSitIn { get; set; }

        [Display(Name = "Date/Time Checkout")]
        public DateTime? TimeCheckOut { get; set; }


        [Display(Name = "Sit-In Duration")]
        public string Duration { get; set; }
    }
}
