using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.Dtos;

public class UserProfileDto : BaseDto
{
    public Guid UserId { get; set; }
    [MaxLength(100)]
    public string ProfileName { get; set; } = string.Empty;
    public ICollection<WatchlistItemDto> WatchListItems { get; set; } = new List<WatchlistItemDto>();
}