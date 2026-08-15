using EcommerceBackend.DTOs.Order;
using EcommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize(Roles = "Customer")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // POST: api/orders
    [HttpPost]
    public async Task<IActionResult> CreateOrder(
        CreateOrderDto request)
    {
        var result =
            await _orderService.CreateOrderAsync(request);

        return Ok(result);
    }

    // GET: api/orders
    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var result =
            await _orderService.GetMyOrdersAsync();

        return Ok(result);
    }

    // GET: api/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(
        Guid id)
    {
        var result =
            await _orderService.GetOrderByIdAsync(id);

        return Ok(result);
    }
}