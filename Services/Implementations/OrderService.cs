using System.Security.Claims;
using EcommerceBackend.Data;
using EcommerceBackend.DTOs.Common;
using EcommerceBackend.DTOs.Order;
using EcommerceBackend.Models;
using EcommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services.Implementations;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderService(
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
                "User is not authenticated.");
        }

        return id;
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(
        CreateOrderDto request)
    {
        var userId = GetUserId();

        // ==============================
        // GET ADDRESS
        // ==============================

        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.Id == request.AddressId &&
                x.UserId == userId);

        if (address == null)
        {
            return ApiResponse<OrderDto>.FailureResponse(
                "Address not found.");
        }

        // ==============================
        // GET CART
        // ==============================

        var cart = await _context.Carts
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return ApiResponse<OrderDto>.FailureResponse(
                "Your cart is empty.");
        }

        // ==============================
        // VALIDATE STOCK
        // ==============================

        foreach (var item in cart.Items)
        {
            if (item.Product == null)
            {
                return ApiResponse<OrderDto>.FailureResponse(
                    "A product in your cart no longer exists.");
            }

            if (!item.Product.IsActive)
            {
                return ApiResponse<OrderDto>.FailureResponse(
                    $"Product '{item.Product.Name}' is no longer available.");
            }

            if (item.Quantity > item.Product.StockQuantity)
            {
                return ApiResponse<OrderDto>.FailureResponse(
                    $"Only {item.Product.StockQuantity} units of '{item.Product.Name}' are available.");
            }
        }

        // ==============================
        // CALCULATE TOTAL
        // ==============================

        decimal subTotal = 0;

        foreach (var item in cart.Items)
        {
            var price = item.Product.DiscountPrice
                        ?? item.Product.Price;

            subTotal += price * item.Quantity;
        }

        decimal shippingAmount = 0;
        decimal discountAmount = 0;

        decimal totalAmount =
            subTotal +
            shippingAmount -
            discountAmount;

        // ==============================
        // CREATE ORDER
        // ==============================

        var order = new Order
        {
            Id = Guid.NewGuid(),

            OrderNumber = GenerateOrderNumber(),

            UserId = userId,

            SubTotal = subTotal,

            ShippingAmount = shippingAmount,

            DiscountAmount = discountAmount,

            TotalAmount = totalAmount,

            Status = "Pending",

            PaymentStatus = "Pending",

            ShippingFullName = address.FullName,

            ShippingPhone = address.Phone,

            ShippingAddressLine1 = address.AddressLine1,

            ShippingAddressLine2 = address.AddressLine2,

            ShippingCity = address.City,

            ShippingState = address.State,

            ShippingPostalCode = address.PostalCode,

            ShippingCountry = address.Country,

            CreatedAt = DateTime.UtcNow
        };

        // ==============================
        // CREATE ORDER ITEMS
        // ==============================

        foreach (var item in cart.Items)
        {
            var price = item.Product.DiscountPrice
                        ?? item.Product.Price;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),

                OrderId = order.Id,

                ProductId = item.ProductId,

                ProductName = item.Product.Name,

                UnitPrice = price,

                Quantity = item.Quantity,

                TotalPrice = price * item.Quantity
            };

            order.Items.Add(orderItem);

            // Reduce stock
            item.Product.StockQuantity -= item.Quantity;
        }

        // ==============================
        // CREATE PAYMENT
        // ==============================

        var payment = new Payment
        {
            Id = Guid.NewGuid(),

            OrderId = order.Id,

            Amount = totalAmount,

            Provider = "PayPal",

            Status = "Pending"
        };

        order.Payment = payment;

        // ==============================
        // SAVE
        // ==============================

        _context.Orders.Add(order);

        // Remove cart items
        _context.CartItems.RemoveRange(cart.Items);

        await _context.SaveChangesAsync();

        // ==============================
        // RESPONSE
        // ==============================

        return ApiResponse<OrderDto>.SuccessResponse(
            MapOrder(order),
            "Order created successfully.");
    }

    // ==========================================
    // GET MY ORDERS
    // ==========================================

    public async Task<ApiResponse<List<OrderDto>>> GetMyOrdersAsync()
    {
        var userId = GetUserId();

        var orders = await _context.Orders
            .Include(x => x.Items)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var result = orders
            .Select(MapOrder)
            .ToList();

        return ApiResponse<List<OrderDto>>.SuccessResponse(
            result,
            "Orders fetched successfully.");
    }

    // ==========================================
    // GET ORDER
    // ==========================================

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(
        Guid orderId)
    {
        var userId = GetUserId();

        var order = await _context.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.Id == orderId &&
                x.UserId == userId);

        if (order == null)
        {
            return ApiResponse<OrderDto>.FailureResponse(
                "Order not found.");
        }

        return ApiResponse<OrderDto>.SuccessResponse(
            MapOrder(order),
            "Order fetched successfully.");
    }

    // ==========================================
    // ORDER NUMBER
    // ==========================================

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }

    // ==========================================
    // MAP
    // ==========================================

    private static OrderDto MapOrder(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,

            OrderNumber = order.OrderNumber,

            SubTotal = order.SubTotal,

            ShippingAmount = order.ShippingAmount,

            DiscountAmount = order.DiscountAmount,

            TotalAmount = order.TotalAmount,

            Status = order.Status,

            PaymentStatus = order.PaymentStatus,

            CreatedAt = order.CreatedAt,

            Items = order.Items
                .Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,

                    ProductName = item.ProductName,

                    UnitPrice = item.UnitPrice,

                    Quantity = item.Quantity,

                    TotalPrice = item.TotalPrice
                })
                .ToList()
        };
    }
}