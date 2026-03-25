using System.ComponentModel.DataAnnotations;

namespace Cosmatics.Models;

public class CountryCode
{
    public int Id { get; set; }
    [Required]
    public string Code { get; set; } = string.Empty; // e.g. "+20"
    [Required]
    public string Name { get; set; } = string.Empty; // e.g. "Egypt"
}
