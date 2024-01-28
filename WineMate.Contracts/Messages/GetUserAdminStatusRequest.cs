namespace WineMate.Contracts.Messages;

public record GetUserAdminStatusRequest
{
    public Guid UserId { get; init; }
}
