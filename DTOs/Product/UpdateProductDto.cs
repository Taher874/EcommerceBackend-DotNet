using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.DTOs.Product;

public class UpdateProductDto
{
    [Required]
    [MaxLength(250)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? DiscountPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    [Required]
    [MaxLength(100)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }

    public bool IsActive { get; set; }

    public List<string> ImageUrls { get; set; } = new();
}