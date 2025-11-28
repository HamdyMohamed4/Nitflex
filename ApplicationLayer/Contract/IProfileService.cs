using ApplicationLayer.Dtos;
using Domains;

namespace ApplicationLayer.Contract;

public interface IProfileService : IBaseService<UserProfile, UserProfileDto>
{
    Task<(bool Status, string Message, IEnumerable<UserProfileDto> Response)> GetAllProfilesByUserIdAsync(Guid userId);
    Task<(bool Status, string Message, UserProfileDto? userProfileDto)> GetProfileByUserIdAsync(Guid userId, Guid profileId);
    Task<(bool Status, string Message, CreateProfileDto userProfileDto)> CreateProfileAsync(Guid userId, CreateProfileDto createProfileDto);
    Task<(bool Status, string Message)> UploadProfilePicture(Guid userId, Guid profileId, FileUploadDto fileUploadDto);
    Task<(bool Status, string Message)> DeleteProfilePicture(Guid userId, Guid profileId, string profilePicturePath);
    Task<(bool Status, string Message)> UpdateProfileAsync(Guid userId, Guid profileId);
    Task<(bool Status, string Message)> DeleteProfileByUserId(Guid userId, Guid profileId);
    Task<(bool Status, string Message, IEnumerable<UserHistoryDto> userHistoryListDto)> GetViewingHistoryAsync(Guid userId, Guid profileId);
}

    Task<UserProfileDto> CreateProfileAsync(CreateProfileDto dto, Guid userId);
    Task<List<UserProfileDto>> GetAllProfilesAsync(Guid userId);
    Task<UserProfileDto?> GetProfileByIdAsync(Guid profileId, Guid userId);
    Task<bool> DeleteProfileAsync(Guid profileId, Guid userId);
    Task<(bool Status, string Message)> TransferProfileToUserAsync(Guid profileId, string targetEmail, Guid callerUserId);
    Task<bool> LockProfileAsync(Guid profileId, Guid userId, string pin);
    Task<bool> UnlockProfileAsync(Guid profileId, Guid userId, string pin);
    Task<bool> ToggleKidModeAsync(Guid profileId, Guid userId, bool enable);
}


