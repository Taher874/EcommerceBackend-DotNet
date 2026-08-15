using System.Security.Claims;
using EcommerceBackend.Data;
using EcommerceBackend.DTOs.Checkout;
using EcommerceBackend.DTOs.Common;
using EcommerceBackend.Models;
using EcommerceBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Services.Implementations;

public class CheckoutService : ICheckoutService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPayPalService _payPalService;

    public CheckoutService(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor,
    IPayPalService payPalService)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _payPalService = payPalService;
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

    public async Task<ApiResponse<CheckoutDto>> CheckoutAsync(
        CheckoutRequestDto request)
    {
        var userId = GetUserId();

        // ==========================================
        // GET ADDRESS
        // ==========================================

        var address = await _context.Addresses
            .FirstOrDefaultAsync(x =>
                x.Id == request.AddressId &&
                x.UserId == userId);

        if (address == null)
        {
            return ApiResponse<CheckoutDto>.FailureResponse(
                "Address not found."
            );
        }

        // ==========================================
        // GET CART
        // ==========================================

        var cart = await _context.Carts
            .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return ApiResponse<CheckoutDto>.FailureResponse(
                "Your cart is empty."
            );
        }

        // ==========================================
        // VALIDATE PRODUCTS & STOCK
        // ==========================================

        foreach (var item in cart.Items)
        {
            var product = item.Product;

            if (product == null || !product.IsActive)
            {
                return ApiResponse<CheckoutDto>.FailureResponse(
                    "One or more products are no longer available."
                );
            }

            if (product.CategoryId == Guid.Empty)
            {
                return ApiResponse<CheckoutDto>.FailureResponse(
                    $"Product '{product.Name}' has an invalid category."
                );
            }

            if (item.Quantity > product.StockQuantity)
            {
                return ApiResponse<CheckoutDto>.FailureResponse(
                    $"Only {product.StockQuantity} units of '{product.Name}' are available."
                );
            }
        }

        // ==========================================
        // CALCULATE TOTAL
        // ==========================================

        decimal subTotal = 0;

        foreach (var item in cart.Items)
        {
            var price = item.Product!.DiscountPrice
                        ?? item.Product.Price;

            subTotal += price * item.Quantity;
        }

        decimal shippingAmount = 0;

        decimal discountAmount = 0;

        decimal totalAmount =
            subTotal +
            shippingAmount -
            discountAmount;

        // ==========================================
        // CREATE ORDER
        // ==========================================

        var order = new Order
        {
            Id = Guid.NewGuid(),

            UserId = userId,

            OrderNumber = GenerateOrderNumber(),

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

            ShippingCountry = address.Country
        };

        // ==========================================
        // CREATE ORDER ITEMS
        // ==========================================

        foreach (var item in cart.Items)
        {
            var product = item.Product!;

            var price = product.DiscountPrice
                        ?? product.Price;

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),

                OrderId = order.Id,

                ProductId = product.Id,

                ProductName = product.Name,

                UnitPrice = price,

                Quantity = item.Quantity,

                TotalPrice = price * item.Quantity
            };

            order.Items.Add(orderItem);
        }

        // ==========================================
        // CREATE PAYMENT
        // ==========================================

        var payment = new Payment
        {
            Id = Guid.NewGuid(),

            OrderId = order.Id,

            Amount = totalAmount,

            Provider = "PayPal",

            Status = "Pending"
        };

        var paypalOrder = await _payPalService.CreateOrderAsync(
            order.TotalAmount,
            order.OrderNumber
        );

        payment.ProviderOrderId = paypalOrder.OrderId;

        _context.Orders.Add(order);

        _context.Payments.Add(payment);

        await _context.SaveChangesAsync();




        // ==========================================
        // RESPONSE
        // ==========================================

        var response = new CheckoutDto
        {
            OrderId = order.Id,

            OrderNumber = order.OrderNumber,

            SubTotal = order.SubTotal,

            ShippingAmount = order.ShippingAmount,

            DiscountAmount = order.DiscountAmount,

            TotalAmount = order.TotalAmount,

            OrderStatus = order.Status,

            PaymentStatus = order.PaymentStatus,

             PaymentUrl = paypalOrder.ApprovalUrl
        };

        return ApiResponse<CheckoutDto>.SuccessResponse(
            response,
            "Checkout created successfully."
        );
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
    }
}