namespace WineMate.Contracts.Api;

public class WineReviewDetailResponse
{
    public Guid Id { get; set; }
    public Guid WineId { get; set; }
    public string? Title { get; set; }
    public string? Body { get; set; }
    public int Rating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
