using System.ComponentModel.DataAnnotations;

namespace TicketInventory.Api.DTOs;

public record CreateEventRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "TotalTickets must be greater than 0.")]
    public int TotalTickets { get; set; }
}
