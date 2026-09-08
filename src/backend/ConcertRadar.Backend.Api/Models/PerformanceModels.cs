using System.ComponentModel.DataAnnotations;

namespace ConcertRadar.Backend.Api.Models;

public record PerformanceResponse(Guid Id, Guid BandId, Guid EventId, DateTime Date);

public class PerformanceRequest
{
	[Required]
	public Guid BandId { get; set; }

	[Required]
	public Guid EventId { get; set; }

	public DateTime Date { get; set; }
}