using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertRadar.Backend.Infrastructure.Persistence.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
	public void Configure(EntityTypeBuilder<Genre> builder)
	{
		builder.ToTable("Genres");

		builder.HasKey(genre => genre.Id);

		builder.Property(genre => genre.Source)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(genre => genre.ExternalId)
			.IsRequired()
			.HasMaxLength(200);

		builder.Property(genre => genre.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(genre => genre.Description)
			.HasMaxLength(1000);

		builder.HasIndex(genre => new { genre.Source, genre.ExternalId })
			.IsUnique();

		builder.HasIndex(genre => genre.Name);
	}
}