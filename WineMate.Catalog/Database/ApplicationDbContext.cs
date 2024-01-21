using Microsoft.EntityFrameworkCore;

using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    public virtual DbSet<Wine> Wines => Set<Wine>();
}
