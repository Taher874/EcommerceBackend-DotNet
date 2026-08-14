using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.DTOs.Category;

public class CreateCategoryDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}