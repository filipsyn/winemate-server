using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Contracts;

public class UpdateWineRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; } = null;
    public int Year { get; set; }
    public WineType Type { get; set; } = WineType.Other;
    public Guid WineMakerId { get; set; }
}
