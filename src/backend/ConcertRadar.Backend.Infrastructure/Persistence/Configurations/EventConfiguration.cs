using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertRadar.Backend.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
	public void Configure(EntityTypeBuilder<Event> builder)
	{
		builder.ToTable("Events");

		builder.HasKey(@event => @event.Id);

		builder.Property(@event => @event.Source)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(@event => @event.ExternalId)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(@event => @event.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(@event => @event.Description)
			.HasMaxLength(2000);

		builder.Property(@event => @event.StartDate)
			.IsRequired();

		builder.Property(@event => @event.EndDate);

		builder.HasOne(@event => @event.Venue)
			.WithMany(venue => venue.Events)
			.HasForeignKey(@event => @event.VenueId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Property(@event => @event.TicketUrl)
			.HasMaxLength(2048);

		builder.HasIndex(@event => @event.StartDate);
		builder.HasIndex(@event => new { @event.Source, @event.ExternalId })
			.IsUnique();
	}
}