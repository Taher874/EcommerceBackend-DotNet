using EcommerceBackend.DTOs.Payment;

namespace EcommerceBackend.Services.Interfaces;

public interface IPayPalService
{
    Task<PayPalCreateOrderResult> CreateOrderAsync(
        decimal amount,
        string orderNumber);

    Task<PayPalCaptureResult> CaptureOrderAsync(
        string paypalOrderId);
}