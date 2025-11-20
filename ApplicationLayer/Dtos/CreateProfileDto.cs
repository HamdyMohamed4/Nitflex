using System.ComponentModel.DataAnnotations;

namespace ApplicationLayer.Contract;

public class CreateProfileDto
{
    public Guid UserId { get; set; }
    [StringLength(20, MinimumLength = 3, ErrorMessage = "Length should be between 3 and 20 characters.")]
    public string ProfileName { get; set; } = string.Empty;
}