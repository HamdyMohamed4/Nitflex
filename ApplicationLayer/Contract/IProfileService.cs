using ApplicationLayer.Dtos;
using Domains;

namespace ApplicationLayer.Contract;

public interface IProfileService : IBaseService<UserProfile, UserProfileDto>
{

    Task<UserProfileDto> CreateProfileAsync(CreateProfileDto dto, Guid userId);
    Task<List<UserProfileDto>> GetAllProfilesAsync(Guid userId);
    Task<UserProfileDto?> GetProfileByIdAsync(Guid profileId, Guid userId);
    Task<bool> DeleteProfileAsync(Guid profileId, Guid userId);
    Task<(bool Status, string Message)> TransferProfileToUserAsync(Guid profileId, string targetEmail, Guid callerUserId);
    Task<bool> LockProfileAsync(Guid profileId, Guid userId, string pin);
    Task<bool> UnlockProfileAsync(Guid profileId, Guid userId, string pin);
    Task<bool> ToggleKidModeAsync(Guid profileId, Guid userId, bool enable);
}


