using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ConcertRadar.Backend.Infrastructure.Persistence;

public class ConcertRadarDbContextFactory : IDesignTimeDbContextFactory<ConcertRadarDbContext>
{
	public ConcertRadarDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<ConcertRadarDbContext>();
		optionsBuilder.UseSqlite("Data Source=concert-radar.db");

		return new ConcertRadarDbContext(optionsBuilder.Options);
	}
}