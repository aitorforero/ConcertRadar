namespace ConcertRadar.Backend.Core.Entities;

public class Event
{
	public Guid Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public string Venue { get; set; } = string.Empty;

	public string? City { get; set; }

	public string? Address { get; set; }

	public string? TicketUrl { get; set; }

	public ICollection<Performance> Performances { get; set; } = new List<Performance>();
}