namespace EcommerceBackend.Models;

public class Order
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = "Pending";

    public string PaymentStatus { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // User
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    // Shipping address snapshot
    public string ShippingFullName { get; set; } = string.Empty;

    public string ShippingPhone { get; set; } = string.Empty;

    public string ShippingAddressLine1 { get; set; } = string.Empty;

    public string? ShippingAddressLine2 { get; set; }

    public string ShippingCity { get; set; } = string.Empty;

    public string ShippingState { get; set; } = string.Empty;

    public string ShippingPostalCode { get; set; } = string.Empty;

    public string ShippingCountry { get; set; } = string.Empty;

    // Relationships
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    public Payment? Payment { get; set; }
}