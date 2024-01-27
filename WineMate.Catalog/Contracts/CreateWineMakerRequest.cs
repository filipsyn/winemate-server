using WineMate.Catalog.Database.Entities;

namespace WineMate.Catalog.Contracts;

public class CreateWineMakerRequest
{
    public string Name { get; set; }
    public Address Address { get; set; }
}
