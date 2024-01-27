using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Contracts;

public class WineMakerDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
}
