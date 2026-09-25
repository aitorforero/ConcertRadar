using System.ComponentModel.DataAnnotations;

namespace ConcertRadar.Backend.Api.Models;

public record UserResponse(Guid Id, string Name, string Email);

public class UpdateProfileRequest
{
	[Required]
	[StringLength(100, MinimumLength = 2)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	[StringLength(320)]
	public string Email { get; set; } = string.Empty;
}

public class RegisterRequest
{
	[Required]
	[StringLength(100, MinimumLength = 2)]
	public string Name { get; set; } = string.Empty;

	[Required]
	[EmailAddress]
	[StringLength(320)]
	public string Email { get; set; } = string.Empty;

	[Required]
	[StringLength(128, MinimumLength = 8)]
	public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
	[Required]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required]
	public string Password { get; set; } = string.Empty;
}

public class ChangePasswordRequest
{
	[Required]
	public string CurrentPassword { get; set; } = string.Empty;

	[Required]
	[StringLength(128, MinimumLength = 8)]
	public string NewPassword { get; set; } = string.Empty;
}

public class DeleteAccountRequest
{
	[Required]
	public string Password { get; set; } = string.Empty;
}