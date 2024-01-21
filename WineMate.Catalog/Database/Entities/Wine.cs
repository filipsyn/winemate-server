namespace WineMate.Catalog.Database.Entities;

public class Wine : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; } = null;
    public required int Year { get; set; }
    public WineType Type { get; set; } = WineType.Other;
}

public enum WineType
{
    Other,
    Red,
    White,
    Rose,
    Sparkling,
    Dessert,
    Fortified
}
