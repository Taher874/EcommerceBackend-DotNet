namespace EcommerceBackend.DTOs.Cart;

public class CartDto
{
    public Guid Id { get; set; }

    public List<CartItemDto> Items { get; set; } = new();

    public int TotalItems { get; set; }

    public decimal SubTotal { get; set; }
}