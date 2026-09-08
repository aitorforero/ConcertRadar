using ConcertRadar.Backend.Api.Models;
using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(ConcertRadarDbContext dbContext) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll(CancellationToken cancellationToken)
	{
		var events = await dbContext.Events.AsNoTracking()
			.Select(@event => ToResponse(@event))
			.ToListAsync(cancellationToken);

		return Ok(events);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<EventResponse>> GetById(Guid id, CancellationToken cancellationToken)
	{
		var @event = await dbContext.Events.AsNoTracking()
			.Where(@event => @event.Id == id)
			.Select(@event => ToResponse(@event))
			.SingleOrDefaultAsync(cancellationToken);

		return @event is null ? NotFound() : Ok(@event);
	}

	[HttpPost]
	public async Task<ActionResult<EventResponse>> Create(EventRequest request, CancellationToken cancellationToken)
	{
		if (request.EndDate < request.StartDate)
		{
			return BadRequest("EndDate no puede ser anterior a StartDate.");
		}

		var @event = new Event
		{
			Id = Guid.NewGuid(), Name = request.Name, Description = request.Description,
			StartDate = request.StartDate, EndDate = request.EndDate, Venue = request.Venue,
			City = request.City, Address = request.Address, TicketUrl = request.TicketUrl
		};
		dbContext.Events.Add(@event);
		await dbContext.SaveChangesAsync(cancellationToken);

		return CreatedAtAction(nameof(GetById), new { id = @event.Id }, ToResponse(@event));
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, EventRequest request, CancellationToken cancellationToken)
	{
		var @event = await dbContext.Events.FindAsync([id], cancellationToken);
		if (@event is null)
		{
			return NotFound();
		}

		if (request.EndDate < request.StartDate)
		{
			return BadRequest("EndDate no puede ser anterior a StartDate.");
		}

		@event.Name = request.Name;
		@event.Description = request.Description;
		@event.StartDate = request.StartDate;
		@event.EndDate = request.EndDate;
		@event.Venue = request.Venue;
		@event.City = request.City;
		@event.Address = request.Address;
		@event.TicketUrl = request.TicketUrl;
		await dbContext.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var @event = await dbContext.Events.FindAsync([id], cancellationToken);
		if (@event is null)
		{
			return NotFound();
		}

		dbContext.Events.Remove(@event);
		await dbContext.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

	private static EventResponse ToResponse(Event @event) =>
		new(@event.Id, @event.Name, @event.Description, @event.StartDate, @event.EndDate,
			@event.Venue, @event.City, @event.Address, @event.TicketUrl);
}