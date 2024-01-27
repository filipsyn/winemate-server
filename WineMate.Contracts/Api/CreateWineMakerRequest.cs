using WineMate.Contracts.Common;

namespace WineMate.Contracts.Api;

public class CreateWineMakerRequest
{
    public string Name { get; set; }
    public Address Address { get; set; }
}
