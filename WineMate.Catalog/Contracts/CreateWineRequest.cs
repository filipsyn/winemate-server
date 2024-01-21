using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Contracts;

public class CreateWineRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; } = null;
    public required int Year { get; set; }
    public WineType Type { get; set; } = WineType.Other;
}
