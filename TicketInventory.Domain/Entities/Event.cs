using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketInventory.Domain.Entities;

public class Event
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
    public int TotalTickets { get; set; }

    public int Sold { get; set; }

    [NotMapped]
    public int Remaining => TotalTickets - Sold;

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
