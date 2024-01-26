namespace WineMate.Catalog.Database.Entities;

public class WineMaker : BaseEntity
{
    public required string Name { get; set; }

    public Address Address { get; set; }
}
