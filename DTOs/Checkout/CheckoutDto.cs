namespace EcommerceBackend.DTOs.Checkout;

public class CheckoutDto
{
    public Guid OrderId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public decimal SubTotal { get; set; }

    public decimal ShippingAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string OrderStatus { get; set; } = string.Empty;

    public string PaymentStatus { get; set; } = string.Empty;

    public string? PaymentUrl { get; set; }
}