using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Infrastructure.Persistence;

public class ConcertRadarDbContext : DbContext
{
	public ConcertRadarDbContext(DbContextOptions<ConcertRadarDbContext> options)
		: base(options)
	{
	}

	public DbSet<Genre> Genres => Set<Genre>();

	public DbSet<Band> Bands => Set<Band>();

	public DbSet<Event> Events => Set<Event>();

	public DbSet<Performance> Performances => Set<Performance>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConcertRadarDbContext).Assembly);
	}
}