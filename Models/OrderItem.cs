namespace EcommerceBackend.Models;

public class OrderItem
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    // Order
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;

    // Product
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;

    // Product snapshot
    public string ProductName { get; set; } = string.Empty;
}