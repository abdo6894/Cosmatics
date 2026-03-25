using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string ImageUrl { get; set; } = string.Empty;
}
