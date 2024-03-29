using WineMate.Contracts.Common;

namespace WineMate.Catalog.Database.Entities;

public class WineMaker : BaseEntity
{
    public required string Name { get; set; }

    public Address Address { get; set; }

    public IList<Wine> Wines { get; set; }
}
