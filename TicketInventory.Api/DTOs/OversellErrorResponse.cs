namespace TicketInventory.Api.DTOs;

public class OversellErrorResponse
{
    public string Message { get; set; } = "Not enough Tickets remaining to fulfill the order.";
    public int RemainingTickets { get; set; }
}
