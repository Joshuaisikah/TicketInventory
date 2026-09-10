using Microsoft.EntityFrameworkCore;
using TicketInventory.Api.DTOs;
using TicketInventory.Domain.Entities;
using TicketInventory.Infrastructure.Data;

namespace TicketInventory.Api.Services;

public enum DeleteEventResult
{
    Success,
    NotFound,
    InUse,
}

public enum CreateEventResult
{
    Success,
    InvalidRequest,
    AlreadyExists,
}

public class EventService : IEventService
{
    private readonly InventoryDbContext _dbContext;

    public EventService(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(CreateEventResult Result, EventDto? Event)> CreateEventAsync(
        CreateEventRequest request,
        CancellationToken cancellationToken
    )
    {
        var ev = new Event
        {
            Name = request.Name,
            Description = request.Description,
            TotalTickets = request.TotalTickets,
        };
        _dbContext.Events.Add(ev);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (CreateEventResult.Success, ToDto(ev));
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Unique constraint violation, event with the same name already exists
            return (CreateEventResult.AlreadyExists, null);
        }
    }

    public async Task<EventDto?> GetEventByIdAsync(
        Guid eventId,
        CancellationToken cancellationToken
    )
    {
        var ev = await _dbContext
            .Events.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        return ev is null ? null : ToDto(ev);
    }

    public async Task<DeleteEventResult> DeleteEventAsync(
        Guid eventId,
        CancellationToken cancellationToken
    )
    {
        var exists = await _dbContext
            .Events.AsNoTracking()
            .AnyAsync(e => e.Id == eventId, cancellationToken);
        if (!exists)
        {
            return DeleteEventResult.NotFound;
        }
        try
        {
            var rows = await _dbContext
                .Events.Where(e => e.Id == eventId)
                .ExecuteDeleteAsync(cancellationToken);
            return rows > 0 ? DeleteEventResult.Success : DeleteEventResult.NotFound;
        }
        catch (Npgsql.PostgresException ex) when (ex.SqlState == "23503")
        {
            // Foreign key violation, event is in use
            return DeleteEventResult.InUse;
        }
    }

    private static EventDto ToDto(Event ev) =>
        new EventDto
        {
            Id = ev.Id,
            Name = ev.Name,
            Description = ev.Description,

            TotalTickets = ev.TotalTickets,
            Sold = ev.Sold,
            Remaining = ev.Remaining,
        };
}
