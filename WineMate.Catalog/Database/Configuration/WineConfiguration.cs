using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Database.Configuration;

public class WineConfiguration : IEntityTypeConfiguration<Wine>
{
    public void Configure(EntityTypeBuilder<Wine> builder)
    {
        builder.HasKey(wine => wine.Id);

        builder.HasOne<WineMaker>()
            .WithMany(maker => maker.Wines)
            .HasForeignKey(wine => wine.WineMakerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
