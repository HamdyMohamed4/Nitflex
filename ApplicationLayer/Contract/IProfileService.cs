using ApplicationLayer.Dtos;
using Domains;

namespace ApplicationLayer.Contract;

public interface IProfileService : IBaseService<UserProfile, UserProfileDto>
{
    Task<(bool Status, string Message, IEnumerable<UserProfileDto> Response)> GetAllProfilesByUserIdAsync(Guid userId);
    Task<(bool Status, string Message, UserProfileDto? userProfileDto)> GetProfileByUserIdAsync(Guid userId, Guid profileId);
    Task<(bool Status, string Message, CreateProfileDto userProfileDto)> CreateProfileAsync(Guid userId, CreateProfileDto createProfileDto);
    Task<(bool Status, string Message)> UpdateProfileAsync(Guid userId, Guid profileId);
    Task<(bool Status, string Message)> DeleteProfileByUserId(Guid userId, Guid profileId);
    Task<(bool Status, string Message, IEnumerable<UserHistoryDto> userHistoryListDto)> GetViewingHistoryAsync(Guid userId, Guid profileId);
}