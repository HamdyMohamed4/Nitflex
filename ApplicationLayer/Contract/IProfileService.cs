using ApplicationLayer.Dtos;
using Domains;

namespace ApplicationLayer.Contract;

public interface IProfileService : IBaseService<UserProfile, UserProfileDto>
{
    Task<(bool Status, string Message, IEnumerable<UserProfileDto> Response)> GetAllProfilesByUserIdAsync(Guid userId);
    Task<(bool Status, string Message, UserProfileDto? userProfileDto)> GetProfileByUserIdAsync(Guid userId, Guid profileId);
    Task<(bool Status, string Message, CreateProfileDto userProfileDto)> CreateProfileAsync(Guid userId, CreateProfileDto createProfileDto);
    Task<(bool Status, string Message)> UpdateProfileAsync(Guid userId, Guid profileId);
    Task<(bool Status, string Message)> DeleteProfileByUserId(Guid profileId, Guid userId);
    Task<(bool Status, string Message, IEnumerable<UserHistoryDto> userHistoryListDto)> GetViewingHistoryAsync(Guid userId, Guid profileId);

    // New: Transfer profile ownership to another user by email.
    // Caller must be the owner of the source profile (enforced by implementation).
    Task<(bool Status, string Message)> TransferProfileToUserAsync(Guid profileId, string targetEmail, Guid callerUserId);
}


