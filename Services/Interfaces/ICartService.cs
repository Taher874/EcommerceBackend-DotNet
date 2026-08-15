using EcommerceBackend.DTOs.Cart;
using EcommerceBackend.DTOs.Common;

namespace EcommerceBackend.Services.Interfaces;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetCartAsync();

    Task<ApiResponse<CartDto>> AddItemAsync(
        AddCartItemDto request);

    Task<ApiResponse<CartDto>> UpdateItemAsync(
        Guid productId,
        int quantity);

    Task<ApiResponse<bool>> RemoveItemAsync(
        Guid productId);

    Task<ApiResponse<bool>> ClearCartAsync();
}