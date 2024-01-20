using Microsoft.EntityFrameworkCore;

namespace WineMate.Catalog.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options) { }
}
