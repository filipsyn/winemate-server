namespace WineMate.Contracts.Api;

public class UpdateWineReviewRequest
{
    public Guid WineId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int Rating { get; set; }
}
