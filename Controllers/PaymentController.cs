using EcommerceBackend.Data;
using EcommerceBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IPayPalService _payPalService;

    public PaymentController(
        AppDbContext context,
        IPayPalService payPalService)
    {
        _context = context;
        _payPalService = payPalService;
    }

    [HttpGet("paypal/success")]
    [AllowAnonymous]
    public async Task<IActionResult> PayPalSuccess(
        [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest("PayPal order ID is missing.");
        }

        // Find our payment using PayPal order ID
        var payment = await _context.Payments
            .Include(x => x.Order)
                .ThenInclude(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.ProviderOrderId == token);

        if (payment == null)
        {
            return NotFound("Payment not found.");
        }

        // Prevent duplicate capture
        if (payment.Status == "Completed")
        {
            return Redirect(
                $"http://localhost:3000/payment/success?orderId={payment.OrderId}"
            );
        }

        // Capture PayPal payment
        var capture = await _payPalService
            .CaptureOrderAsync(token);

        if (!capture.Success)
        {
            payment.Status = "Failed";

            payment.Order.PaymentStatus = "Failed";

            await _context.SaveChangesAsync();

            return Redirect(
                $"http://localhost:3000/payment/failed?orderId={payment.OrderId}"
            );
        }

        // ==========================================
        // PAYMENT SUCCESS
        // ==========================================

        payment.Status = "Completed";

        payment.TransactionId = capture.CaptureId;

        payment.PaymentReference = capture.OrderId;

        payment.PaidAt = DateTime.UtcNow;

        payment.Order.PaymentStatus = "Paid";

        payment.Order.Status = "Confirmed";

        // ==========================================
        // REDUCE STOCK
        // ==========================================

        foreach (var item in payment.Order.Items)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == item.ProductId);

            if (product == null)
            {
                continue;
            }

            product.StockQuantity -= item.Quantity;
        }

        // ==========================================
        // CLEAR CART
        // ==========================================

        var cart = await _context.Carts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x =>
                x.UserId == payment.Order.UserId);

        if (cart != null)
        {
            _context.CartItems.RemoveRange(cart.Items);
        }

        await _context.SaveChangesAsync();

        // ==========================================
        // REDIRECT FRONTEND
        // ==========================================

        return Redirect(
            $"http://localhost:3000/payment/success?orderId={payment.OrderId}"
        );
    }

    [HttpGet("paypal/cancel")]
    [AllowAnonymous]
    public async Task<IActionResult> PayPalCancel(
        [FromQuery] string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Redirect(
                "http://localhost:3000/payment/cancel"
            );
        }

        var payment = await _context.Payments
            .Include(x => x.Order)
            .FirstOrDefaultAsync(x =>
                x.ProviderOrderId == token);

        if (payment != null)
        {
            payment.Status = "Cancelled";

            payment.Order.PaymentStatus = "Cancelled";

            await _context.SaveChangesAsync();
        }

        return Redirect(
            $"http://localhost:3000/payment/cancel?orderId={payment?.OrderId}"
        );
    }
}