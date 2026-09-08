using ConcertRadar.Backend.Api.Models;
using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController(ConcertRadarDbContext dbContext) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<IEnumerable<GenreResponse>>> GetAll(CancellationToken cancellationToken)
	{
		var genres = await dbContext.Genres.AsNoTracking()
			.Select(genre => new GenreResponse(genre.Id, genre.Name, genre.Description))
			.ToListAsync(cancellationToken);

		return Ok(genres);
	}

	[HttpGet("{id:guid}")]
	public async Task<ActionResult<GenreResponse>> GetById(Guid id, CancellationToken cancellationToken)
	{
		var genre = await dbContext.Genres.AsNoTracking()
			.Where(genre => genre.Id == id)
			.Select(genre => new GenreResponse(genre.Id, genre.Name, genre.Description))
			.SingleOrDefaultAsync(cancellationToken);

		return genre is null ? NotFound() : Ok(genre);
	}

	[HttpPost]
	public async Task<ActionResult<GenreResponse>> Create(GenreRequest request, CancellationToken cancellationToken)
	{
		var genre = new Genre { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description };
		dbContext.Genres.Add(genre);
		await dbContext.SaveChangesAsync(cancellationToken);

		var response = ToResponse(genre);
		return CreatedAtAction(nameof(GetById), new { id = genre.Id }, response);
	}

	[HttpPut("{id:guid}")]
	public async Task<IActionResult> Update(Guid id, GenreRequest request, CancellationToken cancellationToken)
	{
		var genre = await dbContext.Genres.FindAsync([id], cancellationToken);
		if (genre is null)
		{
			return NotFound();
		}

		genre.Name = request.Name;
		genre.Description = request.Description;
		await dbContext.SaveChangesAsync(cancellationToken);

		return NoContent();
	}

	[HttpDelete("{id:guid}")]
	public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
	{
		var genre = await dbContext.Genres.FindAsync([id], cancellationToken);
		if (genre is null)
		{
			return NotFound();
		}

		dbContext.Genres.Remove(genre);
		try
		{
			await dbContext.SaveChangesAsync(cancellationToken);
		}
		catch (DbUpdateException)
		{
			return Conflict("No se puede eliminar un genero que tiene bandas asociadas.");
		}

		return NoContent();
	}

	private static GenreResponse ToResponse(Genre genre) =>
		new(genre.Id, genre.Name, genre.Description);
}