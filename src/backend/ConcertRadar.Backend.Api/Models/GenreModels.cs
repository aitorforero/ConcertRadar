using System.ComponentModel.DataAnnotations;

namespace ConcertRadar.Backend.Api.Models;

public record GenreResponse(Guid Id, string Name, string? Description);

public class GenreRequest
{
	[Required]
	[StringLength(100)]
	public string Name { get; set; } = string.Empty;

	[StringLength(1000)]
	public string? Description { get; set; }
}