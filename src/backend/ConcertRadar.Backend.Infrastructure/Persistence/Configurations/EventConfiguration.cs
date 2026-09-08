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

		builder.Property(@event => @event.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(@event => @event.Description)
			.HasMaxLength(2000);

		builder.Property(@event => @event.StartDate)
			.IsRequired();

		builder.Property(@event => @event.EndDate);

		builder.Property(@event => @event.Venue)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(@event => @event.City)
			.HasMaxLength(100);

		builder.Property(@event => @event.Address)
			.HasMaxLength(500);

		builder.Property(@event => @event.TicketUrl)
			.HasMaxLength(2048);

		builder.HasIndex(@event => @event.StartDate);
	}
}