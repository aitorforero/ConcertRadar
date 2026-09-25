using System.Security.Claims;
using ConcertRadar.Backend.Api.Models;
using ConcertRadar.Backend.Api.Services;
using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(ConcertRadarDbContext dbContext, PasswordService passwordService) : ControllerBase
{
	[HttpPost("register")]
	public async Task<ActionResult<UserResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
	{
		var email = NormalizeEmail(request.Email);
		if (await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
		{
			return Conflict("No se puede crear la cuenta con esos datos.");
		}

		var user = new User
		{
			Id = Guid.NewGuid(),
			Name = request.Name.Trim(),
			Email = email,
			PasswordHash = passwordService.Hash(request.Password),
			CreatedAt = DateTime.UtcNow
		};

		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(cancellationToken);
		await SignInAsync(user);

		return Ok(ToResponse(user));
	}

	[HttpPost("login")]
	public async Task<ActionResult<UserResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
	{
		var email = NormalizeEmail(request.Email);
		var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);
		if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
		{
			return Unauthorized("Email o contraseña incorrectos.");
		}

		await SignInAsync(user);
		return Ok(ToResponse(user));
	}

	[Authorize]
	[HttpGet("me")]
	public async Task<ActionResult<UserResponse>> GetCurrentUser(CancellationToken cancellationToken)
	{
		var user = await GetCurrentUserAsync(cancellationToken);
		return user is null ? Unauthorized() : Ok(ToResponse(user));
	}

	[Authorize]
	[HttpPut("me")]
	public async Task<IActionResult> UpdateCurrentUser(UpdateProfileRequest request, CancellationToken cancellationToken)
	{
		var user = await GetCurrentUserAsync(cancellationToken);
		if (user is null)
		{
			return Unauthorized();
		}

		var email = NormalizeEmail(request.Email);
		if (email != user.Email)
		{
			return BadRequest("El email es el identificador y no puede modificarse.");
		}

		user.Name = request.Name.Trim();

		await dbContext.SaveChangesAsync(cancellationToken);
		return Ok(ToResponse(user));
	}

	[Authorize]
	[HttpPut("me/password")]
	public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
	{
		var user = await GetCurrentUserAsync(cancellationToken);
		if (user is null)
		{
			return Unauthorized();
		}

		if (!passwordService.Verify(request.CurrentPassword, user.PasswordHash))
		{
			return BadRequest("La contraseña actual no es correcta.");
		}

		user.PasswordHash = passwordService.Hash(request.NewPassword);
		await dbContext.SaveChangesAsync(cancellationToken);
		return NoContent();
	}

	[Authorize]
	[HttpDelete("me")]
	public async Task<IActionResult> DeleteCurrentUser(DeleteAccountRequest request, CancellationToken cancellationToken)
	{
		var user = await GetCurrentUserAsync(cancellationToken);
		if (user is null)
		{
			return Unauthorized();
		}

		if (!passwordService.Verify(request.Password, user.PasswordHash))
		{
			return BadRequest("La contraseña no es correcta.");
		}

		dbContext.Users.Remove(user);
		await dbContext.SaveChangesAsync(cancellationToken);
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return NoContent();
	}

	[Authorize]
	[HttpPost("logout")]
	public async Task<IActionResult> Logout()
	{
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
		return NoContent();
	}

	private async Task<User?> GetCurrentUserAsync(CancellationToken cancellationToken)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		return Guid.TryParse(userId, out var id)
			? await dbContext.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken)
			: null;
	}

	private async Task SignInAsync(User user)
	{
		var claims = new[]
		{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email),
			new Claim(ClaimTypes.Name, user.Name)
		};
		var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		await HttpContext.SignInAsync(
			CookieAuthenticationDefaults.AuthenticationScheme,
			new ClaimsPrincipal(identity));
	}

	private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

	private static UserResponse ToResponse(User user) => new(user.Id, user.Name, user.Email);
}