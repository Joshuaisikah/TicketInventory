using TicketInventory.Api.DTOs;

namespace TicketInventory.Api.Services;

public enum PurchaseResult
{
    Success,
    NotEnoughTickets,
    EventNotFound,
}

public interface IPurchaseService
{
    public Task<(PurchaseResult Result, PurchaseResponse? Response, int? Remaining)> PurchaseAsync(
        Guid EventId,
        int Quantity,
        CancellationToken ct
    );
}
