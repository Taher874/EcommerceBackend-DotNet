namespace EcommerceBackend.Models;

public class Payment
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public string? TransactionId { get; set; }

    public string? ProviderOrderId { get; set; }

    public string? PaymentReference { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }

    // Order
    public Guid OrderId { get; set; }

    public Order Order { get; set; } = null!;
}