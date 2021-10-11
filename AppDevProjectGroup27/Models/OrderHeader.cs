using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models
{
    public class OrderHeader
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        public double OrderTotalOriginal { get; set; }

        [Required]
        [DisplayFormat(DataFormatString ="{0:C}")]
        [Display(Name = "Order Total")]
        public double OrderTotal { get; set; }

        [Required]
        [Display(Name = "Pickup Time")]
        public DateTime PickUpTime { get; set; }

        [Required]
        [NotMapped]
        public DateTime PickUpDate { get; set; }

        [Display(Name = "Coupon Code")]
        public string CouponCode { get; set; }

        public double CouponCodeDiscount { get; set; }

        public string Status { get; set; }

        public string PaymentStatus { get; set; }

        public string Comments { get; set; }

        
        [Display(Name = "Pickup Name")]
        public string PickUpName { get; set; }

        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        public string TransactionId { get; set; }


        [Display(Name = "Date/Time Started")]
        public DateTime? StartDateTime { get; set; }
        [Display(Name = "Date/Time Food Ready")]
        public DateTime? CompleteDateTime { get; set; }
        [Display(Name = "Date/Time Order Picked Up")]
        public DateTime? PickedUpOrder { get; set; }

        [Display(Name = "Preparation Duration")]
        public string Duration { get; set; }

        [Display(Name = "ETA")]
        public string EstimatedTimeComplete { get; set; }


        [Display(Name ="Started By")]
        public string OrderStartedBy { get; set; }
        [Display(Name = "Completed By")]
        public string OrderCompletedBy { get; set; }
        [Display(Name = "Cancelled By")]
        public string OrderCancelledBy { get; set; }
        [Display(Name = "Refunded By")]
        public string OrderRefundedBy { get; set; }
    }
}
