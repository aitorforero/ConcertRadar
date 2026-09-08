using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using ConcertRadar.Backend.Scraper.Scraping;
using Microsoft.EntityFrameworkCore;

namespace ConcertRadar.Backend.Scraper.Importing;

public sealed class ConcertImporter(ConcertRadarDbContext dbContext)
{
	public async Task ImportAsync(
		IReadOnlyCollection<ScrapedEvent> scrapedEvents,
		CancellationToken cancellationToken)
	{
		await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

		foreach (var scrapedEvent in scrapedEvents)
		{
			var @event = await UpsertEventAsync(scrapedEvent, cancellationToken);
			foreach (var scrapedPerformance in scrapedEvent.Performances)
			{
				var genre = await UpsertGenreAsync(scrapedEvent.Source, scrapedPerformance, cancellationToken);
				var band = await UpsertBandAsync(scrapedEvent.Source, scrapedPerformance, genre, cancellationToken);
				await UpsertPerformanceAsync(scrapedEvent.Source, scrapedPerformance, band, @event, cancellationToken);
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await transaction.CommitAsync(cancellationToken);
	}

	private async Task<Event> UpsertEventAsync(ScrapedEvent source, CancellationToken cancellationToken)
	{
		var @event = await dbContext.Events.SingleOrDefaultAsync(
			eventEntity => eventEntity.Source == source.Source && eventEntity.ExternalId == source.ExternalId,
			cancellationToken);

		if (@event is null)
		{
			@event = new Event { Id = Guid.NewGuid(), Source = source.Source, ExternalId = source.ExternalId };
			dbContext.Events.Add(@event);
		}

		@event.Name = source.Name;
		@event.Description = source.Description;
		@event.StartDate = source.StartDate;
		@event.EndDate = source.EndDate;
		@event.Venue = source.Venue;
		@event.City = source.City;
		@event.Address = source.Address;
		@event.TicketUrl = source.TicketUrl;
		return @event;
	}

	private async Task<Genre> UpsertGenreAsync(string source, ScrapedPerformance performance, CancellationToken cancellationToken)
	{
		var genre = await dbContext.Genres.SingleOrDefaultAsync(
			entity => entity.Source == source && entity.ExternalId == performance.GenreExternalId,
			cancellationToken);
		if (genre is null)
		{
			genre = new Genre { Id = Guid.NewGuid(), Source = source, ExternalId = performance.GenreExternalId };
			dbContext.Genres.Add(genre);
		}

		genre.Name = performance.GenreName;
		return genre;
	}

	private async Task<Band> UpsertBandAsync(string source, ScrapedPerformance performance, Genre genre, CancellationToken cancellationToken)
	{
		var band = await dbContext.Bands.SingleOrDefaultAsync(
			entity => entity.Source == source && entity.ExternalId == performance.BandExternalId,
			cancellationToken);
		if (band is null)
		{
			band = new Band { Id = Guid.NewGuid(), Source = source, ExternalId = performance.BandExternalId };
			dbContext.Bands.Add(band);
		}

		band.Name = performance.BandName;
		band.GenreId = genre.Id;
		return band;
	}

	private async Task<Performance> UpsertPerformanceAsync(
		string source,
		ScrapedPerformance scrapedPerformance,
		Band band,
		Event @event,
		CancellationToken cancellationToken)
	{
		var performance = await dbContext.Performances.SingleOrDefaultAsync(
			entity => entity.Source == source && entity.ExternalId == scrapedPerformance.ExternalId,
			cancellationToken);
		if (performance is null)
		{
			performance = new Performance
			{
				Id = Guid.NewGuid(), Source = source, ExternalId = scrapedPerformance.ExternalId
			};
			dbContext.Performances.Add(performance);
		}

		performance.BandId = band.Id;
		performance.EventId = @event.Id;
		performance.Date = scrapedPerformance.Date;
		return performance;
	}
}