using EcommerceBackend.DTOs.Checkout;
using EcommerceBackend.DTOs.Common;

namespace EcommerceBackend.Services.Interfaces;

public interface ICheckoutService
{
    Task<ApiResponse<CheckoutDto>> CheckoutAsync(
        CheckoutRequestDto request);
}