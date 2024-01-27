using WineMate.Contracts.Common;

namespace WineMate.Contracts.Api;

public class WineDetailResponse
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; } = null;
    public int Year { get; set; }
    public WineType Type { get; set; } = WineType.Other;
    public Guid WineMakerId { get; set; }
}
