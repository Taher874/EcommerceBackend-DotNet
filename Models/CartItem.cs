namespace EcommerceBackend.Models;

public class CartItem
{
    public Guid Id { get; set; }

    public int Quantity { get; set; }

    // Cart
    public Guid CartId { get; set; }

    public Cart Cart { get; set; } = null!;

    // Product
    public Guid ProductId { get; set; }

    public Product Product { get; set; } = null!;
}