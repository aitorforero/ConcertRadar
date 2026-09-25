using ConcertRadar.Backend.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ConcertRadar.Backend.Infrastructure.Persistence.Configurations;

public class UserBandFollowConfiguration : IEntityTypeConfiguration<UserBandFollow>
{
	public void Configure(EntityTypeBuilder<UserBandFollow> builder)
	{
		builder.ToTable("UserBandFollows");

		builder.HasKey(follow => new { follow.UserId, follow.BandId });

		builder.HasOne(follow => follow.User)
			.WithMany()
			.HasForeignKey(follow => follow.UserId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasOne(follow => follow.Band)
			.WithMany()
			.HasForeignKey(follow => follow.BandId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}