namespace TicketInventory.Api.DTOs;

public class PurchaseResponse
{
    public Guid OrderId { get; set; }
    public Guid EventId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; }
    public int RemainingTickets { get; set; }
}
