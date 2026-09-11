using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TicketInventory.Api.DTOs;
using TicketInventory.Api.Services;

namespace TicketInventory.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly IEventService _eventService;

    public EventsController(IPurchaseService purchaseService, IEventService eventService)
    {
        _purchaseService = purchaseService;
        _eventService = eventService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventRequest request, CancellationToken ct)
    {
        var (result, eventDto) = await _eventService.CreateEventAsync(request, ct);
        if (result == CreateEventResult.Success && eventDto == null)
        {
            return Problem("Event was created but no event data was returned");
        }
        ;
        return result switch
        {
            CreateEventResult.Success => CreatedAtAction(
                nameof(GetEvent),
                new { id = eventDto!.Id },
                eventDto
            ),
            CreateEventResult.AlreadyExists => Conflict("Event with the same name already exists"),
            _ => Problem("An unexpected error occurred while creating the event"),
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventDto>> GetEvent(Guid id, CancellationToken ct)
    {
        var eventDto = await _eventService.GetEventByIdAsync(id, ct);
        return eventDto is not null ? Ok(eventDto) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken ct)
    {
        var result = await _eventService.DeleteEventAsync(id, ct);
        return result switch
        {
            DeleteEventResult.Success => NoContent(),
            DeleteEventResult.NotFound => NotFound(),
            DeleteEventResult.InUse => Conflict("Cannot delete event as it has associated orders"),
            _ => Problem("An unexpected error occurred while deleting the event"),
        };
    }

    [HttpPost("{id:guid}/purchase")]
    public async Task<IActionResult> PurchaseTickets(
        Guid id,
        PurchaseRequest request,
        CancellationToken ct
    )
    {
        var (result, response, remaining) = await _purchaseService.PurchaseAsync(
            id,
            request.Quantity,
            ct
        );
        return result switch
        {
            PurchaseResult.Success => CreatedAtAction(nameof(GetEvent), new { id }, response),
            PurchaseResult.NotEnoughTickets => Conflict(
                new { Message = "Not enough tickets available", Remaining = remaining }
            ),
            PurchaseResult.EventNotFound => NotFound(),
            _ => Problem("An unexpected error occurred while processing the purchase"),
        };
    }
}
