namespace EcommerceBackend.Models;

public class Cart
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // User
    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    // Items
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}