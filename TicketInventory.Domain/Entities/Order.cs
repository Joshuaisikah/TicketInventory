using System.ComponentModel.DataAnnotations;

namespace TicketInventory.Domain.Entities;

public class Order
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid EventId { get; set; }

    public Event Event { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public OrderStatus Status { get; set; } = OrderStatus.Confirmed;
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Cancelled,
}
