using System.Security.Claims;
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
	public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll(
		[FromQuery] string? search,
		[FromQuery] string? city,
		[FromQuery] DateTime? fromDate,
		[FromQuery] DateTime? toDate,
		CancellationToken cancellationToken)
	{
		if (fromDate.HasValue && toDate.HasValue && toDate.Value.Date < fromDate.Value.Date)
		{
			return BadRequest("toDate no puede ser anterior a fromDate.");
		}

		var query = dbContext.Events.AsNoTracking()
			.Include(@event => @event.Venue)
			.Include(@event => @event.Performances)
			.ThenInclude(performance => performance.Band)
			.AsQueryable();

		if (!string.IsNullOrWhiteSpace(city) && !city.Equals("Todas", StringComparison.OrdinalIgnoreCase))
		{
			query = query.Where(@event => @event.Venue != null && @event.Venue.City != null && @event.Venue.City == city);
		}

		if (fromDate.HasValue)
		{
			query = query.Where(@event => @event.StartDate >= fromDate.Value.Date);
		}

		if (toDate.HasValue)
		{
			query = query.Where(@event => @event.StartDate < toDate.Value.Date.AddDays(1));
		}

		if (!string.IsNullOrWhiteSpace(search))
		{
			var normalized = search.Trim();
			query = query.Where(@event =>
				@event.Name.Contains(normalized) ||
				(@event.Venue != null && @event.Venue.Name.Contains(normalized)) ||
				(@event.Venue != null && @event.Venue.City != null && @event.Venue.City.Contains(normalized)));
		}

		var followedBandIds = await GetFollowedBandIdsAsync(cancellationToken);
		var events = await query
			.OrderBy(@event => @event.StartDate)
			.ToListAsync(cancellationToken);

		return Ok(events.Select(@event => ToResponse(@event, followedBandIds)));
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<EventResponse>> GetById(Guid id, CancellationToken cancellationToken)
	{
		var @event = await dbContext.Events.AsNoTracking()
			.Include(@event => @event.Venue)
			.Include(@event => @event.Performances)
			.ThenInclude(performance => performance.Band)
			.Where(@event => @event.Id == id)
			.SingleOrDefaultAsync(cancellationToken);

		if (@event is null)
		{
			return NotFound();
		}

		var followedBandIds = await GetFollowedBandIdsAsync(cancellationToken);
		return Ok(ToResponse(@event, followedBandIds));
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
			StartDate = request.StartDate, EndDate = request.EndDate,
			Venue = await GetOrCreateVenueAsync(request, cancellationToken), TicketUrl = request.TicketUrl
		};
		dbContext.Events.Add(@event);
		await dbContext.SaveChangesAsync(cancellationToken);

		return CreatedAtAction(nameof(GetById), new { id = @event.Id }, ToResponse(@event, new HashSet<Guid>()));
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
		@event.Venue = await GetOrCreateVenueAsync(request, cancellationToken);
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

	private async Task<HashSet<Guid>> GetFollowedBandIdsAsync(CancellationToken cancellationToken)
	{
		if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
		{
			return [];
		}

		return await dbContext.UserBandFollows
			.Where(follow => follow.UserId == userId)
			.Select(follow => follow.BandId)
			.ToHashSetAsync(cancellationToken);
	}

	private static EventResponse ToResponse(Event @event, IReadOnlySet<Guid> followedBandIds) =>
		new(@event.Id, @event.Name, @event.Description, @event.StartDate, @event.EndDate,
			@event.Venue?.Name ?? "Sin venue",
			@event.Venue?.City,
			@event.Venue?.Address,
			@event.TicketUrl,
			@event.Performances
				.Select(performance => performance.Band)
				.Where(band => band is not null)
				.DistinctBy(band => band.Id)
				.Select(band => new ArtistResponse(band.Id, band.Name, followedBandIds.Contains(band.Id)))
				.ToArray());

	private async Task<Venue> GetOrCreateVenueAsync(EventRequest request, CancellationToken cancellationToken)
	{
		var venue = await dbContext.Venues.SingleOrDefaultAsync(
			candidate => candidate.Name == request.Venue && candidate.City == request.City && candidate.Address == request.Address,
			cancellationToken);
		if (venue is not null)
		{
			return venue;
		}

		venue = new Venue
		{
			Id = Guid.NewGuid(), Name = request.Venue, City = request.City, Address = request.Address
		};
		dbContext.Venues.Add(venue);
		return venue;
	}
}