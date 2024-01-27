using Microsoft.EntityFrameworkCore;

using WineMate.Reviews.Database.Entities;

namespace WineMate.Reviews.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    public DbSet<WineReview> WineReviews => Set<WineReview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
