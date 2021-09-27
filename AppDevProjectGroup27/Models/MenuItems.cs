using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace AppDevProjectGroup27.Models
{
    public class MenuItems
    {
        public int Id { get; set; }

        [Required]
        public string  Name { get; set; }

        public string Descriptions { get; set; }

        public string  Spicyness { get; set; }
        public enum ESpicy 
        {
          NA=0,
          NotSpicy = 1,
          Spicy=2,
          VerySpicy=3
        }

        public string Image { get; set; }

        [Display(Name= "Category")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]// Referencing the table with the key field
        public virtual Category Category { get; set; }

        [Display(Name="Sub-Category Name")]
        public int SubCategoryId { get; set; }
        [ForeignKey("SubCategoryId")] // Referencing the table with the key field
        public virtual SubCategory SubCategory { get; set; }

        [Range(1, int.MaxValue, ErrorMessage ="Price should be greater than R{1}")]
        public double Price { get; set; }

        [Display(Name = "On Special")]
        public bool OnSpecial { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Display(Name = "Available Quantity")]
        [Required]
        public int AvaQuantity { get; set; }

        [Display(Name = "Estimated Preparation Duration [mins]")]
        [Required]
        [Range(0, int.MaxValue)]
        public int ETAEstimate { get; set; }
    }
}
