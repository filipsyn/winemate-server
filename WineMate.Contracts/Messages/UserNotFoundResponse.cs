namespace WineMate.Contracts.Messages;

public record UserNotFoundResponse
{
    public Guid UserId { get; init; }
}
