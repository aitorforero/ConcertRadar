namespace ConcertRadar.Backend.Core.Entities;

public class Band
{
	public Guid Id { get; set; }

	public string Name { get; set; } = string.Empty;

	public string? Description { get; set; }

	public Guid GenreId { get; set; }

	public Genre Genre { get; set; } = null!;

	public string? Country { get; set; }

	public string? City { get; set; }

	public DateTime? FoundedDate { get; set; }

	public string? ImageUrl { get; set; }

	public string? WebsiteUrl { get; set; }

	public ICollection<Performance> Performances { get; set; } = new List<Performance>();
}
