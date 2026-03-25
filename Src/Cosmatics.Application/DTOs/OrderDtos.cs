using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;


public record CreateOrderDto
{
    [Required(ErrorMessage = "Payment Method is required.")]
    public string PaymentMethod { get; set; } = string.Empty;
} 

public record OrderDto(int OrderId, DateTime OrderDate, string Status, decimal TotalAmount);
