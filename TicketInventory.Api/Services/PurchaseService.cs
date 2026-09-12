using Microsoft.EntityFrameworkCore;
using TicketInventory.Api.DTOs;
using TicketInventory.Domain.Entities;
using TicketInventory.Infrastructure.Data;

namespace TicketInventory.Api.Services;

public class PurchaseService : IPurchaseService
{
    private readonly InventoryDbContext _dbContext;

    public PurchaseService(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(
        PurchaseResult Result,
        PurchaseResponse? Response,
        int? Remaining
    )> PurchaseAsync(Guid Eventid, int Quantity, CancellationToken ct)
    {
        var exist = await _dbContext.Events.AsNoTracking().AnyAsync(e => e.Id == Eventid, ct);
        if (!exist)
        {
            return (PurchaseResult.EventNotFound, null, null);
        }
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        var rowsAffected = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE ""Events"" SET ""Sold"" =""Sold"" + {Quantity} WHERE ""Id"" = {Eventid} AND (""TotalTickets"" - ""Sold"") >= {Quantity}",
            ct
        );
        /** var rowsAffected = await _dbContext.Event
       .Where(e => e.Id == eventId && e.TotalTickets - e.Sold >= quantity)
       .ExecuteUpdateAsync(s => s.SetProperty(e => e.Sold, e => e.Sold + quantity), ct);
   This is the Linq version of the update operation, but it may not be supported in all database providers. The raw SQL version is more universally compatible.
   However it is more prone to SQL injection if not used carefully, so always use parameterized queries or\ interpolated\ strings as shown above.
   The Linq versionis more readable and maintainable also modern.
   **/
        if (rowsAffected == 0)
        {
            await transaction.RollbackAsync(ct);
            var current = await _dbContext
                .Events.AsNoTracking()
                .Where(e => e.Id == Eventid)
                .Select(e => (int?)( e.TotalTickets - e.Sold))
                .FirstOrDefaultAsync(ct);
            return current is null
                ? (PurchaseResult.EventNotFound, null, null)
                : (PurchaseResult.NotEnoughTickets, null, current);
        }
        var order = new Order { EventId = Eventid, Quantity = Quantity };
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        var remaining = await _dbContext
            .Events.AsNoTracking()
            .Where(e => e.Id == Eventid)
            .Select(e => e.TotalTickets - e.Sold)
            .FirstAsync(ct);
        var Response = new PurchaseResponse
        {
            OrderId = order.Id,
            EventId = order.EventId,
            Quantity = order.Quantity,
            Status = order.Status.ToString(),
            PurchasedAt = order.PurchasedAt,
            RemainingTickets = remaining,
        };
        return (PurchaseResult.Success, Response, remaining);
    }
}
