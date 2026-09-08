namespace ConcertRadar.Backend.Core.Entities;

public class Genre
{
	public Guid Id { get; set; }

	public string Source { get; set; } = "manual";

	public string ExternalId { get; set; } = Guid.NewGuid().ToString("N");

	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }

	public ICollection<Band> Bands { get; set; } = new List<Band>();
}
