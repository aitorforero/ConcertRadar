namespace ConcertRadar.Backend.Infrastructure.Persistence;

public static class ConcertRadarDatabase
{
	private const string DatabaseFileName = "concert-radar.db";

	public static string Resolve(string contentRootPath)
	{
		var configuredPath = Environment.GetEnvironmentVariable("CONCERT_RADAR_DB_PATH");
		if (!string.IsNullOrWhiteSpace(configuredPath))
		{
			return Path.GetFullPath(configuredPath);
		}

		var candidates = new[]
		{
			Path.Combine(contentRootPath, "..", "ConcertRadar.Backend.Infrastructure", DatabaseFileName),
			Path.Combine(contentRootPath, "src", "backend", "ConcertRadar.Backend.Infrastructure", DatabaseFileName),
			Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ConcertRadar.Backend.Infrastructure", DatabaseFileName)
		};

		return candidates
			.Select(Path.GetFullPath)
			.FirstOrDefault(File.Exists)
			?? Path.GetFullPath(candidates[0]);
	}
}