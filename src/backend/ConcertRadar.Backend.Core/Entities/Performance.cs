namespace ConcertRadar.Backend.Core.Entities;

public class Performance
{
	public Guid Id { get; set; }

	public string Source { get; set; } = "manual";

	public string ExternalId { get; set; } = Guid.NewGuid().ToString("N");

	public Guid BandId { get; set; }

	public Band Band { get; set; } = null!;

	public Guid EventId { get; set; }

	public Event Event { get; set; } = null!;

	public DateTime Date { get; set; }
}