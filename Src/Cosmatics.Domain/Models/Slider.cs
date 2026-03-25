using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Models
{
    public class Slider
    {
        public int Id { get; set; }
        
        [Required]
        public string CouponCode { get; set; }
        
        [Range(0, 100)]
        public int DiscountPercent { get; set; }
        
        [Required]
        public string DescriptionTitle1 { get; set; }
        
        public string DescriptionTitle2 { get; set; }
        
        public string ImageUrl { get; set; }
    }
}
