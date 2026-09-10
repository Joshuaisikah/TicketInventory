using TicketInventory.Api.DTOs;

namespace TicketInventory.Api.Services;

public interface IEventService
{
    Task<(CreateEventResult Result, EventDto? Event)> CreateEventAsync(
        CreateEventRequest request,
        CancellationToken cancellationToken
    );
    Task<EventDto?> GetEventByIdAsync(Guid eventId, CancellationToken cancellationToken);
    Task<DeleteEventResult> DeleteEventAsync(Guid eventId, CancellationToken cancellationToken);
}
