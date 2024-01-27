namespace WineMate.Contracts.Messages;

public record GetWineStatusResponse
{
    public Guid WineId { get; init; }
    public bool Exists { get; init; }
}
