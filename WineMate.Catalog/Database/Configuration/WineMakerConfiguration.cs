using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Database.Configuration;

public class WineMakerConfiguration : IEntityTypeConfiguration<WineMaker>
{
    public void Configure(EntityTypeBuilder<WineMaker> builder)
    {
        builder.HasKey(wineMaker => wineMaker.Id);

        builder.HasMany<Wine>(maker => maker.Wines)
            .WithOne(wine => wine.WineMaker)
            .HasForeignKey(wine => wine.WineMakerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsOne(wineMaker => wineMaker.Address);
    }
}
