using EcommerceBackend.DTOs.Cart;
using EcommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    // ==========================================
    // GET CART
    // ==========================================

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var response = await _cartService.GetCartAsync();

        return Ok(response);
    }

    // ==========================================
    // ADD ITEM
    // ==========================================

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddCartItemDto request)
    {
        var response =
            await _cartService.AddItemAsync(request);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    // ==========================================
    // UPDATE ITEM
    // ==========================================

    [HttpPut("items/{productId:guid}")]
    public async Task<IActionResult> UpdateItem(
        Guid productId,
        [FromBody] int quantity)
    {
        var response =
            await _cartService.UpdateItemAsync(
                productId,
                quantity
            );

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    // ==========================================
    // REMOVE ITEM
    // ==========================================

    [HttpDelete("items/{productId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid productId)
    {
        var response =
            await _cartService.RemoveItemAsync(productId);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    // ==========================================
    // CLEAR CART
    // ==========================================

    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var response =
            await _cartService.ClearCartAsync();

        return Ok(response);
    }
}