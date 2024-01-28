namespace WineMate.Identity.Database.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateTime? LastLoginAt { get; set; } = null;
    public bool IsAdmin { get; set; } = false;
}
