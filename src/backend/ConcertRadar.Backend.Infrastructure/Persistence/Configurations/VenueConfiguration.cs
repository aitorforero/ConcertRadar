using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertRadar.Backend.Infrastructure.Persistence.Configurations;

public class VenueConfiguration : IEntityTypeConfiguration<Venue>
{
	public void Configure(EntityTypeBuilder<Venue> builder)
	{
		builder.ToTable("Venues");

		builder.HasKey(venue => venue.Id);

		builder.Property(venue => venue.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(venue => venue.Address)
			.HasMaxLength(500);

		builder.Property(venue => venue.Province)
			.HasMaxLength(100);

		builder.Property(venue => venue.City)
			.HasMaxLength(100);

		builder.Property(venue => venue.Country)
			.HasMaxLength(100);
	}
}