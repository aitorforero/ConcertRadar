using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Infrastructure.Persistence;

public class ConcertRadarDbContext : DbContext
{
	public ConcertRadarDbContext(DbContextOptions<ConcertRadarDbContext> options)
		: base(options)
	{
	}

	public DbSet<Band> Bands => Set<Band>();

	public DbSet<Event> Events => Set<Event>();

	public DbSet<Venue> Venues => Set<Venue>();

	public DbSet<Performance> Performances => Set<Performance>();

	public DbSet<User> Users => Set<User>();

	public DbSet<UserBandFollow> UserBandFollows => Set<UserBandFollow>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConcertRadarDbContext).Assembly);
	}
}