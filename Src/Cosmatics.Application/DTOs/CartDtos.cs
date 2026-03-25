using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

public class CartItemDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public string ProductName { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
    
    [Required]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}

public record CartDto(List<CartItemDto> Items, decimal Total);
