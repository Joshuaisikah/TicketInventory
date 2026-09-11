using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketInventory.Api.DTOs;
using TicketInventory.Domain.Entities;
using TicketInventory.Infrastructure.Data;
using Xunit;

namespace TicketInventory.Tests.Integration;

// Runs against the REAL Postgres instance (docker-compose), not InMemory.
// InMemory doesn't implement row-level locking, so it can't exercise the
// atomic UPDATE ... WHERE pattern's actual concurrency guarantee — the
// test would pass even against a broken naive implementation.
public class PurchaseConcurrencyTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PurchaseConcurrencyTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task FiftyConcurrentPurchases_AgainstTenStock_ExactlyTenSucceed()
    {
        // Arrange: seed a fresh event directly via DbContext, bypassing HTTP,
        // so the test starts from a known, isolated state.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var seedEvent = new Event
        {
            Name = $"Concurrency Test Event {Guid.NewGuid()}",
            TotalTickets = 10,
            Sold = 0,
        };
        db.Events.Add(seedEvent);
        await db.SaveChangesAsync();

        var eventId = seedEvent.Id;

        // Act: fire 50 concurrent purchase requests of 1 ticket each.
        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync(
                $"/api/events/{eventId}/purchase",
                new PurchaseRequest { Quantity = 1 });
            return response;
        });

        var responses = await Task.WhenAll(tasks);

        // Assert: exactly 10 succeeded (201), exactly 40 were rejected (409).
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);

        successCount.Should().Be(10);
        conflictCount.Should().Be(40);

        // Assert: the database's Sold count matches what actually succeeded.
        using var verifyScope = _factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        var finalEvent = await verifyDb.Events.AsNoTracking()
            .FirstAsync(e => e.Id == eventId);

        finalEvent.Sold.Should().Be(10);
        finalEvent.Sold.Should().BeLessThanOrEqualTo(finalEvent.TotalTickets);

        // Assert: sum of all order quantities for this event equals Sold.
        var orderQuantitySum = await verifyDb.Orders
            .Where(o => o.EventId == eventId)
            .SumAsync(o => o.Quantity);

        orderQuantitySum.Should().Be(finalEvent.Sold);
    }
}
