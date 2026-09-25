namespace ConcertRadar.Backend.Core.Entities;

public class UserBandFollow
{
	public Guid UserId { get; set; }

	public User User { get; set; } = null!;

	public Guid BandId { get; set; }

	public Band Band { get; set; } = null!;
}