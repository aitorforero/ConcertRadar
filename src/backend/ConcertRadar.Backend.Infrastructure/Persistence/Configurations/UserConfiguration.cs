using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertRadar.Backend.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
	public void Configure(EntityTypeBuilder<User> builder)
	{
		builder.ToTable("Users");

		builder.HasKey(user => user.Id);

		builder.Property(user => user.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(user => user.Email)
			.IsRequired()
			.HasMaxLength(320);

		builder.Property(user => user.PasswordHash)
			.IsRequired()
			.HasMaxLength(500);

		builder.Property(user => user.CreatedAt)
			.IsRequired();

		builder.HasIndex(user => user.Email)
			.IsUnique();
	}
}