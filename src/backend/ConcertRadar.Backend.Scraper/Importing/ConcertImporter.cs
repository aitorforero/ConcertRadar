using ConcertRadar.Backend.Core.Entities;
using ConcertRadar.Backend.Infrastructure.Persistence;
using ConcertRadar.Backend.Scraper.Scraping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConcertRadar.Backend.Scraper.Importing;

public sealed class ConcertImporter(
	ConcertRadarDbContext dbContext,
	ILogger<ConcertImporter> logger)
{
	public async Task ImportAsync(
		IReadOnlyCollection<ScrapedEvent> scrapedEvents,
		CancellationToken cancellationToken)
	{
		await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

		foreach (var scrapedEvent in scrapedEvents)
		{
			logger.LogInformation("Importing event {EventName} ({EventSource}:{EventExternalId})", scrapedEvent.Name, scrapedEvent.Source, scrapedEvent.ExternalId);
			
			var @event = await UpsertEventAsync(scrapedEvent, cancellationToken);
			foreach (var scrapedPerformance in scrapedEvent.Performances)
			{
				var band = await UpsertBandAsync(scrapedPerformance, cancellationToken);
				await UpsertPerformanceAsync(scrapedPerformance, band, @event, cancellationToken);
			}
			
		}
		await dbContext.SaveChangesAsync(cancellationToken);
		await transaction.CommitAsync(cancellationToken);

	}

	private async Task<Event> UpsertEventAsync(ScrapedEvent source, CancellationToken cancellationToken)
	{
		var @event = await dbContext.Events.SingleOrDefaultAsync(
			eventEntity => eventEntity.Source == source.Source 
						&& eventEntity.ExternalId == source.ExternalId,
			cancellationToken);

		if (@event is null)
		{
			@event = new Event { Id = Guid.NewGuid(), Source = source.Source, ExternalId = source.ExternalId };
			dbContext.Events.Add(@event);
		}

		@event.Name = source.Name;
		@event.StartDate = source.StartDate;
		@event.EndDate = source.EndDate;
		@event.Venue = await UpsertVenueAsync(source, cancellationToken);
		return @event;
	}

	private async Task<Venue> UpsertVenueAsync(ScrapedEvent source, CancellationToken cancellationToken)
	{
		var venue = dbContext.Venues.Local.SingleOrDefault(
			candidate => candidate.Name == source.Venue && candidate.City == source.City)
			?? await dbContext.Venues.SingleOrDefaultAsync(
				candidate => candidate.Name == source.Venue && candidate.City == source.City,
				cancellationToken);
		if (venue is null)
		{
			venue = new Venue { Id = Guid.NewGuid(), Name = source.Venue };
			dbContext.Venues.Add(venue);
		}

		venue.City = source.City;
		venue.Address = source.Address;
		return venue;
	}

	private async Task<Band> UpsertBandAsync(ScrapedPerformance performance, CancellationToken cancellationToken)
	{
		var band = dbContext.Bands.Local.SingleOrDefault(
			entity => entity.Name == performance.BandName)
			?? await dbContext.Bands.SingleOrDefaultAsync(
				entity => entity.Name == performance.BandName,
				cancellationToken);
		if (band is null)
		{
			band = new Band { Id = Guid.NewGuid() };
			dbContext.Bands.Add(band);
		}

		band.Name = performance.BandName;
		return band;
	}

	private async Task<Performance> UpsertPerformanceAsync(
		ScrapedPerformance scrapedPerformance,
		Band band,
		Event @event,
		CancellationToken cancellationToken)
	{
		var performance = dbContext.Performances.Local.SingleOrDefault(
			entity => entity.BandId == band.Id &&
				entity.EventId == @event.Id &&
				entity.Date == scrapedPerformance.Date)
			?? await dbContext.Performances.SingleOrDefaultAsync(
				entity => entity.BandId == band.Id &&
					entity.EventId == @event.Id &&
					entity.Date == scrapedPerformance.Date,
				cancellationToken);
		if (performance is null)
		{
			performance = new Performance
			{
				Id = Guid.NewGuid()
			};
			dbContext.Performances.Add(performance);
		}

		performance.BandId = band.Id;
		performance.EventId = @event.Id;
		performance.Date = scrapedPerformance.Date;
		return performance;
	}
}