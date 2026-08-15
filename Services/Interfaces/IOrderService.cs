using EcommerceBackend.DTOs.Common;
using EcommerceBackend.DTOs.Order;

namespace EcommerceBackend.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> CreateOrderAsync(
        CreateOrderDto request);

    Task<ApiResponse<List<OrderDto>>> GetMyOrdersAsync();

    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(
        Guid orderId);
}