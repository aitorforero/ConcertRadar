using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertRadar.Backend.Infrastructure.Persistence.Configurations;

public class PerformanceConfiguration : IEntityTypeConfiguration<Performance>
{
	public void Configure(EntityTypeBuilder<Performance> builder)
	{
		builder.ToTable("Performances");

		builder.HasKey(performance => performance.Id);

		builder.Property(performance => performance.BandId)
			.IsRequired();

		builder.Property(performance => performance.EventId)
			.IsRequired();

		builder.Property(performance => performance.Date)
			.IsRequired();

		builder.HasIndex(performance => performance.Date);
		builder.HasIndex(performance => performance.BandId);
		builder.HasIndex(performance => performance.EventId);

		builder.HasOne(performance => performance.Band)
			.WithMany(band => band.Performances)
			.HasForeignKey(performance => performance.BandId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(performance => performance.Event)
			.WithMany(@event => @event.Performances)
			.HasForeignKey(performance => performance.EventId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}