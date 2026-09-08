using System.ComponentModel.DataAnnotations;

namespace ConcertRadar.Backend.Api.Models;

public record BandResponse(
	Guid Id,
	string Name,
	string? Description,
	Guid GenreId,
	string? Country,
	string? City,
	DateTime? FoundedDate,
	string? ImageUrl,
	string? WebsiteUrl);

public class BandRequest
{
	[Required]
	[StringLength(200)]
	public string Name { get; set; } = string.Empty;

	[StringLength(2000)]
	public string? Description { get; set; }

	[Required]
	public Guid GenreId { get; set; }

	[StringLength(100)]
	public string? Country { get; set; }

	[StringLength(100)]
	public string? City { get; set; }

	public DateTime? FoundedDate { get; set; }

	[StringLength(2048)]
	[Url]
	public string? ImageUrl { get; set; }

	[StringLength(2048)]
	[Url]
	public string? WebsiteUrl { get; set; }
}