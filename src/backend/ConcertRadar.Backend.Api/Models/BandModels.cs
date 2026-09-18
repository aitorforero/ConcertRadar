using System.ComponentModel.DataAnnotations;

namespace ConcertRadar.Backend.Api.Models;

public record BandResponse(
	Guid Id,
	string Name);

public class BandRequest
{
	[Required]
	[StringLength(200)]
	public string Name { get; set; } = string.Empty;

}