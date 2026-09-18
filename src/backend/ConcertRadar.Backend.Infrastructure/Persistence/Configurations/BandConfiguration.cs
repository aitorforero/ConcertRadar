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

		builder.Property(band => band.Name)
			.IsRequired()
			.HasMaxLength(200);

		builder.HasIndex(band => band.Name);

	}
}