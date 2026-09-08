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
		if (!await dbContext.Genres.AnyAsync(genre => genre.Id == request.GenreId, cancellationToken))
		{
			return BadRequest("El genero indicado no existe.");
		}

		var band = new Band
		{
			Id = Guid.NewGuid(), Name = request.Name, Description = request.Description,
			GenreId = request.GenreId, Country = request.Country, City = request.City,
			FoundedDate = request.FoundedDate, ImageUrl = request.ImageUrl, WebsiteUrl = request.WebsiteUrl
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

		if (!await dbContext.Genres.AnyAsync(genre => genre.Id == request.GenreId, cancellationToken))
		{
			return BadRequest("El genero indicado no existe.");
		}

		band.Name = request.Name;
		band.Description = request.Description;
		band.GenreId = request.GenreId;
		band.Country = request.Country;
		band.City = request.City;
		band.FoundedDate = request.FoundedDate;
		band.ImageUrl = request.ImageUrl;
		band.WebsiteUrl = request.WebsiteUrl;
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
		new(band.Id, band.Name, band.Description, band.GenreId, band.Country, band.City,
			band.FoundedDate, band.ImageUrl, band.WebsiteUrl);
}