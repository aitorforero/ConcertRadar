using System.ComponentModel.DataAnnotations;

namespace ConcertRadar.Backend.Api.Models;

public record EventResponse(
	Guid Id,
	string Name,
	string? Description,
	DateTime StartDate,
	DateTime? EndDate,
	string Venue,
	string? City,
	string? Address,
	string? TicketUrl);

public class EventRequest
{
	[Required]
	[StringLength(200)]
	public string Name { get; set; } = string.Empty;

	[StringLength(2000)]
	public string? Description { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	[Required]
	[StringLength(200)]
	public string Venue { get; set; } = string.Empty;

	[StringLength(100)]
	public string? City { get; set; }

	[StringLength(500)]
	public string? Address { get; set; }

	[StringLength(2048)]
	[Url]
	public string? TicketUrl { get; set; }
}