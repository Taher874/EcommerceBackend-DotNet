namespace EcommerceBackend.DTOs.Product;

public class ProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public int StockQuantity { get; set; }

    public string SKU { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}