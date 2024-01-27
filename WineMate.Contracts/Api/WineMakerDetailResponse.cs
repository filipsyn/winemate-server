using WineMate.Contracts.Common;

namespace WineMate.Contracts.Api;

public class WineMakerDetailResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = null!;
}
