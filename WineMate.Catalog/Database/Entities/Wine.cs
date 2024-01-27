using WineMate.Contracts.Common;

namespace WineMate.Catalog.Database.Entities;

public class Wine : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; } = null;
    public required int Year { get; set; }
    public WineType Type { get; set; } = WineType.Other;
    public Guid WineMakerId { get; set; }
    public WineMaker? WineMaker { get; set; }
}
