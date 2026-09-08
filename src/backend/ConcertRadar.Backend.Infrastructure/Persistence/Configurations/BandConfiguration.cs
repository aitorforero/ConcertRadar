using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertRadar.Backend.Infrastructure.Persistence.Configurations;

public class BandConfiguration : IEntityTypeConfiguration<Band>
{
	public void Configure(EntityTypeBuilder<Band> builder)
	{
		builder.ToTable("Bands");

		builder.HasKey(band => band.Id);

		builder.Property(band => band.Source)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(band => band.ExternalId)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(band => band.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(band => band.Description)
			.HasMaxLength(2000);

		builder.Property(band => band.GenreId)
			.IsRequired();

		builder.Property(band => band.Country)
			.HasMaxLength(100);

		builder.Property(band => band.City)
			.HasMaxLength(100);

		builder.Property(band => band.ImageUrl)
			.HasMaxLength(2048);

		builder.Property(band => band.WebsiteUrl)
			.HasMaxLength(2048);

		builder.HasIndex(band => band.Name);
		builder.HasIndex(band => new { band.Source, band.ExternalId })
			.IsUnique();

		builder.HasOne(band => band.Genre)
			.WithMany(genre => genre.Bands)
			.HasForeignKey(band => band.GenreId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}