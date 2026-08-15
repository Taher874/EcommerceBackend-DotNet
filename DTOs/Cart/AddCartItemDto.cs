using System.ComponentModel.DataAnnotations;

namespace EcommerceBackend.DTOs.Cart;

public class AddCartItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;
}