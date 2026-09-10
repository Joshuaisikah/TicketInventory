namespace TicketInventory.Api.DTOs;

public class EventDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TotalTickets { get; set; }
    public int Sold { get; set; }
    public int Remaining { get; set; }
}
