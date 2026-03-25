using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Application.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

public class CreateCategoryDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string ImageUrl { get; set; } = string.Empty;
}

public class UpdateCategoryDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string ImageUrl { get; set; } = string.Empty;
}
