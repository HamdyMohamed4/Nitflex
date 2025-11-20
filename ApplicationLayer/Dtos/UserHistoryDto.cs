using Domains;

namespace ApplicationLayer.Dtos;

public class UserHistoryDto : BaseDto
{
    public Guid ProfileId { get; set; }
    public ContentType ContentType { get; set; }
    public Guid ContentId { get; set; }
    public DateTime LastWatched { get; set; }
}