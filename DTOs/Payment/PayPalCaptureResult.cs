namespace EcommerceBackend.DTOs.Payment;

public class PayPalCaptureResult
{
    public bool Success { get; set; }

    public string OrderId { get; set; } = string.Empty;

    public string? CaptureId { get; set; }

    public string Status { get; set; } = string.Empty;
}