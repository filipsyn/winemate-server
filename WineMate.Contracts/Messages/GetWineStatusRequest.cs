namespace WineMate.Contracts.Messages;

public record GetWineStatusRequest
{
    public Guid WineId { get; init; }
}
