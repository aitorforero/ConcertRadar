using ConcertRadar.Backend.Api.Models;
using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BandsController(ConcertRadarDbContext dbContext) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<IEnumerable<BandResponse>>> GetAll(CancellationToken cancellationToken)
	{
		var bands = await dbContext.Bands.AsNoTracking()
			.Select(band => ToResponse(band))
			.ToListAsync(cancellationToken);

		return Ok(bands);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<BandResponse>> GetById(Guid id, CancellationToken cancellationToken)
	{
		var band = await dbContext.Bands.AsNoTracking()
			.Where(band => band.Id == id)
			.Select(band => ToResponse(band))
			.SingleOrDefaultAsync(cancellationToken);

		return band is null ? NotFound() : Ok(band);
	}

	[HttpPost]
	public async Task<ActionResult<BandResponse>> Create(BandRequest request, CancellationToken cancellationToken)
	{
		var band = new Band
		{
			Id = Guid.NewGuid(), Name = request.Name
		};
		dbContext.Bands.Add(band);
		await dbContext.SaveChangesAsync(cancellationToken);

		return CreatedAtAction(nameof(GetById), new { id = band.Id }, ToResponse(band));
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, BandRequest request, CancellationToken cancellationToken)
	{
		var band = await dbContext.Bands.FindAsync([id], cancellationToken);
		if (band is null)
		{
			return NotFound();
		}

		band.Name = request.Name;
		await dbContext.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var band = await dbContext.Bands.FindAsync([id], cancellationToken);
		if (band is null)
		{
			return NotFound();
		}

		dbContext.Bands.Remove(band);
		try
		{
			await dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException)
		{
			return Conflict("No se puede eliminar una banda que tiene actuaciones asociadas.");
		}

		return NoContent();
	}

	private static BandResponse ToResponse(Band band) =>
		new(band.Id, band.Name);
}