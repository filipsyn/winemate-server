namespace WineMate.Contracts.Messages;

public record GetUserAdminStatusResponse
{
    public Guid UserId { get; init; }
    public bool Exists { get; init; }
    public bool IsAdmin { get; init; }
}
