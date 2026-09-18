namespace ConcertRadar.Backend.Core.Entities;

public class Band
{
	public Guid Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public ICollection<Performance> Performances { get; set; } = new List<Performance>();
}
