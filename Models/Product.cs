namespace EcommerceBackend.Models;

public class Product
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public int StockQuantity { get; set; }

    public string SKU { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Category
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    // Images
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    // Order items
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Cart items
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}