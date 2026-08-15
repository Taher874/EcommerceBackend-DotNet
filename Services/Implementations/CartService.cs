using System.Security.Claims;
using EcommerceBackend.Data;
using EcommerceBackend.DTOs.Cart;
using EcommerceBackend.DTOs.Common;
using EcommerceBackend.Models;
using EcommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services.Implementations;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CartService(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private Guid GetUserId()
    {
        var userId = _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException(
                "User is not authenticated."
            );
        }

        return id;
    }

    // ==========================================
    // GET CART
    // ==========================================

    public async Task<ApiResponse<CartDto>> GetCartAsync()
    {
        var userId = GetUserId();

        var cart = await GetOrCreateCartAsync(userId);

        return ApiResponse<CartDto>.SuccessResponse(
            MapCart(cart),
            "Cart fetched successfully."
        );
    }

    // ==========================================
    // ADD ITEM
    // ==========================================

    public async Task<ApiResponse<CartDto>> AddItemAsync(
    AddCartItemDto request)
{
    var userId = GetUserId();

    // Find product
    var product = await _context.Products
        .Include(x => x.Images)
        .Include(x => x.Category)
        .FirstOrDefaultAsync(x =>
            x.Id == request.ProductId &&
            x.IsActive &&
            x.Category.IsActive);

    if (product == null)
    {
        return ApiResponse<CartDto>.FailureResponse(
            "Product not found."
        );
    }

    if (product.StockQuantity <= 0)
    {
        return ApiResponse<CartDto>.FailureResponse(
            "Product is out of stock."
        );
    }

    // Check requested quantity
    if (request.Quantity > product.StockQuantity)
    {
        return ApiResponse<CartDto>.FailureResponse(
            $"Only {product.StockQuantity} items are available."
        );
    }

    // Find cart
    var cart = await _context.Carts
        .FirstOrDefaultAsync(x => x.UserId == userId);

    // Create cart if it doesn't exist
    if (cart == null)
    {
        cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);

        await _context.SaveChangesAsync();
    }

    // Find existing cart item
    var existingItem = await _context.CartItems
        .FirstOrDefaultAsync(x =>
            x.CartId == cart.Id &&
            x.ProductId == request.ProductId);

    if (existingItem != null)
    {
        var newQuantity =
            existingItem.Quantity + request.Quantity;

        if (newQuantity > product.StockQuantity)
        {
            return ApiResponse<CartDto>.FailureResponse(
                $"Only {product.StockQuantity} items are available."
            );
        }

        existingItem.Quantity = newQuantity;
    }
    else
    {
        var cartItem = new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = request.Quantity
        };

        _context.CartItems.Add(cartItem);
    }

    await _context.SaveChangesAsync();

    // Load updated cart
    var updatedCart = await _context.Carts
        .Include(x => x.Items)
            .ThenInclude(x => x.Product)
                .ThenInclude(x => x.Images)
        .FirstAsync(x => x.Id == cart.Id);

    return ApiResponse<CartDto>.SuccessResponse(
        MapCart(updatedCart),
        "Product added to cart successfully."
    );
}

    // ==========================================
    // UPDATE ITEM
    // ==========================================

    public async Task<ApiResponse<CartDto>> UpdateItemAsync(
        Guid productId,
        int quantity)
    {
        if (quantity <= 0)
        {
            return ApiResponse<CartDto>.FailureResponse(
                "Quantity must be greater than zero."
            );
        }

        var userId = GetUserId();

        var cart = await GetOrCreateCartAsync(userId);

        var item = cart.Items
            .FirstOrDefault(x => x.ProductId == productId);

        if (item == null)
        {
            return ApiResponse<CartDto>.FailureResponse(
                "Product is not in the cart."
            );
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == productId);

        if (product == null || !product.IsActive)
        {
            return ApiResponse<CartDto>.FailureResponse(
                "Product not found."
            );
        }

        if (quantity > product.StockQuantity)
        {
            return ApiResponse<CartDto>.FailureResponse(
                $"Only {product.StockQuantity} items are available."
            );
        }

        item.Quantity = quantity;

        await _context.SaveChangesAsync();

        cart = await GetOrCreateCartAsync(userId);

        return ApiResponse<CartDto>.SuccessResponse(
            MapCart(cart),
            "Cart updated successfully."
        );
    }

    // ==========================================
    // REMOVE ITEM
    // ==========================================

    public async Task<ApiResponse<bool>> RemoveItemAsync(
        Guid productId)
    {
        var userId = GetUserId();

        var cart = await GetOrCreateCartAsync(userId);

        var item = cart.Items
            .FirstOrDefault(x => x.ProductId == productId);

        if (item == null)
        {
            return ApiResponse<bool>.FailureResponse(
                "Product is not in the cart."
            );
        }

        _context.CartItems.Remove(item);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(
            true,
            "Product removed from cart."
        );
    }

    // ==========================================
    // CLEAR CART
    // ==========================================

    public async Task<ApiResponse<bool>> ClearCartAsync()
    {
        var userId = GetUserId();

        var cart = await GetOrCreateCartAsync(userId);

        _context.CartItems.RemoveRange(cart.Items);

        await _context.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(
            true,
            "Cart cleared successfully."
        );
    }

    // ==========================================
    // GET / CREATE CART
    // ==========================================

   private async Task<Cart> GetOrCreateCartAsync(Guid userId)
{
    var cart = await _context.Carts
        .Include(x => x.Items)
            .ThenInclude(x => x.Product)
                .ThenInclude(x => x.Images)
        .FirstOrDefaultAsync(x => x.UserId == userId);

    if (cart != null)
        return cart;

    cart = new Cart
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        CreatedAt = DateTime.UtcNow
    };

    _context.Carts.Add(cart);

    await _context.SaveChangesAsync();

    return cart;
}

    // ==========================================
    // MAP
    // ==========================================

    private static CartDto MapCart(Cart cart)
    {
        var items = cart.Items
            .Where(x => x.Product != null)
            .Select(x =>
            {
                var price = x.Product.DiscountPrice
                    ?? x.Product.Price;

                var image = x.Product.Images
                    .FirstOrDefault()?.ImageUrl;

                return new CartItemDto
                {
                    ProductId = x.ProductId,

                    ProductName = x.Product.Name,

                    ImageUrl = image,

                    Price = x.Product.Price,

                    DiscountPrice = x.Product.DiscountPrice,

                    Quantity = x.Quantity,

                    TotalPrice = price * x.Quantity
                };
            })
            .ToList();

        return new CartDto
        {
            Id = cart.Id,

            Items = items,

            TotalItems = items.Sum(x => x.Quantity),

            SubTotal = items.Sum(x => x.TotalPrice)
        };
    }
}