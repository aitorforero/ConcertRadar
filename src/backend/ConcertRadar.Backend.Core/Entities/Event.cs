namespace ConcertRadar.Backend.Core.Entities;

public class Event
{
	public Guid Id { get; set; }

	public string Source { get; set; } = "manual";

	public string ExternalId { get; set; } = Guid.NewGuid().ToString("N");

	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public Guid VenueId { get; set; }

	public Venue Venue { get; set; } = null!;

	public string? TicketUrl { get; set; }

	public ICollection<Performance> Performances { get; set; } = new List<Performance>();
}