namespace WineMate.Reviews.Database.Entities;

public class WineReview : BaseEntity
{
    public Guid WineId { get; set; }
    public string? Title { get; set; }
    public string Body { get; set; }
    public string Rating { get; set; }
}
