using System.Security.Claims;
using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/bands/{bandId:guid}/follow")]
public class BandFollowsController(ConcertRadarDbContext dbContext) : ControllerBase
{
	[HttpPost]
	public async Task<IActionResult> Follow(Guid bandId, CancellationToken cancellationToken)
	{
		if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
		{
			return Unauthorized();
		}

		if (!await dbContext.Bands.AnyAsync(band => band.Id == bandId, cancellationToken))
		{
			return NotFound("El artista no existe.");
		}

		if (await dbContext.UserBandFollows.AnyAsync(follow => follow.UserId == userId && follow.BandId == bandId, cancellationToken))
		{
			return Conflict("Ya sigues a este artista.");
		}

		dbContext.UserBandFollows.Add(new UserBandFollow { UserId = userId, BandId = bandId });
		await dbContext.SaveChangesAsync(cancellationToken);
		return NoContent();
	}

	[HttpDelete]
	public async Task<IActionResult> Unfollow(Guid bandId, CancellationToken cancellationToken)
	{
		if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
		{
			return Unauthorized();
		}

		var follow = await dbContext.UserBandFollows.SingleOrDefaultAsync(
			candidate => candidate.UserId == userId && candidate.BandId == bandId,
			cancellationToken);
		if (follow is null)
		{
			return NotFound("No sigues a este artista.");
		}

		dbContext.UserBandFollows.Remove(follow);
		await dbContext.SaveChangesAsync(cancellationToken);
		return NoContent();
	}
}