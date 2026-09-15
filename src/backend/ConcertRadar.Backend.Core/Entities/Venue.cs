namespace ConcertRadar.Backend.Core.Entities;

public class Venue
{
	public Guid Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Address { get; set; }

	public string? Province { get; set; }

	public string? City { get; set; }

	public string? Country { get; set; }

	public ICollection<Event> Events { get; set; } = new List<Event>();
}