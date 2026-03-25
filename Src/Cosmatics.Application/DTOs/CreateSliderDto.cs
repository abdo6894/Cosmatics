using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

    public class CreateSliderDto
    {
        [Required]
        public string CouponCode { get; set; }
        
        [Range(0, 100)]
        public int DiscountPercent { get; set; }
        
        [Required]
        public string DescriptionTitle1 { get; set; }
        
        public string DescriptionTitle2 { get; set; }
        
        public string ImageUrl { get; set; }
    }
public class SliderDto
{
    public int Id { get; set; }
    public string CouponCode { get; set; }
    public int DiscountPercent { get; set; }
    public string DescriptionTitle1 { get; set; }
    public string DescriptionTitle2 { get; set; }
    public string ImageUrl { get; set; }
}

public class UpdateSliderDto
{
    public string CouponCode { get; set; }

    [Range(0, 100)]
    public int? DiscountPercent { get; set; }

    public string DescriptionTitle1 { get; set; }

    public string DescriptionTitle2 { get; set; }

    public string ImageUrl { get; set; }
}