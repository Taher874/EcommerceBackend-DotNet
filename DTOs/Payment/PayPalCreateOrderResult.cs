namespace EcommerceBackend.DTOs.Payment;

public class PayPalCreateOrderResult
{
    public string OrderId { get; set; } = string.Empty;

    public string ApprovalUrl { get; set; } = string.Empty;
}