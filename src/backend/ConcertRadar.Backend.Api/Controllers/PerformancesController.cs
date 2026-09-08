using ConcertRadar.Backend.Api.Models;
using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PerformancesController(ConcertRadarDbContext dbContext) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<IEnumerable<PerformanceResponse>>> GetAll(CancellationToken cancellationToken)
	{
		var performances = await dbContext.Performances.AsNoTracking()
			.Select(performance => ToResponse(performance))
			.ToListAsync(cancellationToken);

		return Ok(performances);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<PerformanceResponse>> GetById(Guid id, CancellationToken cancellationToken)
	{
		var performance = await dbContext.Performances.AsNoTracking()
			.Where(performance => performance.Id == id)
			.Select(performance => ToResponse(performance))
			.SingleOrDefaultAsync(cancellationToken);

		return performance is null ? NotFound() : Ok(performance);
	}

	[HttpPost]
	public async Task<ActionResult<PerformanceResponse>> Create(PerformanceRequest request, CancellationToken cancellationToken)
	{
		if (!await dbContext.Bands.AnyAsync(band => band.Id == request.BandId, cancellationToken))
		{
			return BadRequest("La banda indicada no existe.");
		}

		if (!await dbContext.Events.AnyAsync(@event => @event.Id == request.EventId, cancellationToken))
		{
			return BadRequest("El evento indicado no existe.");
		}

		var performance = new Performance
		{
			Id = Guid.NewGuid(), BandId = request.BandId, EventId = request.EventId, Date = request.Date
		};
		dbContext.Performances.Add(performance);
		await dbContext.SaveChangesAsync(cancellationToken);

		return CreatedAtAction(nameof(GetById), new { id = performance.Id }, ToResponse(performance));
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, PerformanceRequest request, CancellationToken cancellationToken)
	{
		var performance = await dbContext.Performances.FindAsync([id], cancellationToken);
		if (performance is null)
		{
			return NotFound();
		}

		if (!await dbContext.Bands.AnyAsync(band => band.Id == request.BandId, cancellationToken) ||
			!await dbContext.Events.AnyAsync(@event => @event.Id == request.EventId, cancellationToken))
		{
			return BadRequest("La banda o el evento indicado no existe.");
		}

		performance.BandId = request.BandId;
		performance.EventId = request.EventId;
		performance.Date = request.Date;
		await dbContext.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var performance = await dbContext.Performances.FindAsync([id], cancellationToken);
		if (performance is null)
		{
			return NotFound();
		}

		dbContext.Performances.Remove(performance);
		await dbContext.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

	private static PerformanceResponse ToResponse(Performance performance) =>
		new(performance.Id, performance.BandId, performance.EventId, performance.Date);
}