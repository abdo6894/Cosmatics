using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

public record ProductCreateDto([Required] string Name, string Description, [Range(0.01, double.MaxValue)] decimal Price, int Stock, string ImageUrl);

public record ProductUpdateDto(
    [Required] string Name, 
    string Description, 
    [Range(0.01, double.MaxValue)] decimal Price, 
    [Range(0, int.MaxValue)] int Stock, 
    string ImageUrl
);
