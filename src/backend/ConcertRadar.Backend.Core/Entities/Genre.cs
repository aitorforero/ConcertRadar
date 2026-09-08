namespace ConcertRadar.Backend.Core.Entities;

public class Genre
{
	public Guid Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }

	public ICollection<Band> Bands { get; set; } = new List<Band>();
}
