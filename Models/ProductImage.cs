namespace EcommerceBackend.Models;

public class ProductImage
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsPrimary { get; set; }

    // Product
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
}