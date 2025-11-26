using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services;

public class ProfileService : BaseService<UserProfile, UserProfileDto>, IProfileService
{

    private readonly ILogger<ProfileService> _logger;

    public ProfileService(IGenericRepository<UserProfile> repo, IMapper mapper, ILogger<ProfileService> logger)
        : base(repo, mapper)
    {

        _logger = logger;
    }

    public async Task<(bool Status, string Message, IEnumerable<UserProfileDto> Response)> GetAllProfilesByUserIdAsync(Guid userId)
    {
        ApplicationUser? user = await _userService.GetUserByIdWithProfilesAsync(userId);

        if (user is null)
            return (false, "User Not Found", null!);

        List<UserProfile> userProfiles = user.Profiles.ToList();

        return (true, "Success", _mapper.Map<List<UserProfileDto>>(userProfiles));
    }

    public async Task<(bool Status, string Message, UserProfileDto? userProfileDto)> GetProfileByUserIdAsync(Guid userId, Guid profileId)
    {
        ApplicationUser? user = await _userService.GetUserByIdWithProfilesAsync(userId);

        if (user is null)
            return (false, "User Not Found", null!);

        UserProfile? userProfile = user.Profiles.FirstOrDefault(x => x.Id == profileId);

        if (userProfile is null)
            return (false, "Profile Not Found", null!);

        UserProfileDto userProfileDto = _mapper.Map<UserProfileDto>(userProfile);

        return (true, "Success", userProfileDto);
    }

    //public async Task<(bool Status, string Message, CreateProfileDto userProfileDto)> CreateProfileAsync(Guid userId, CreateProfileDto createProfileDto)
    //{
    //    ApplicationUser? user = await _userService.GetUserByIdentityAsync(userId.ToString());

    //    if (user is null)
    //        return (false, "User Not Found", null!);

    //    UserProfile userProfile = new UserProfile { UserId = userId, ProfileName = createProfileDto.ProfileName };

    //    var result = await _repo.Add(userProfile);

    //    if (result.Item1 == true)
    //        return (true, $"{result.Item2}", createProfileDto);

    //    else
    //        return (false, $"Could not create profile for user with Id {userId}", null!);
    //}



    public async Task<(bool Status, string Message, CreateProfileDto userProfileDto)> CreateProfileAsync(Guid userId, CreateProfileDto createProfileDto)
    {


        // Check if user already has a profile with same name
        var exists = await _repo.AnyAsync(p => p.UserId == userId && p.ProfileName == createProfileDto.ProfileName);
        if (exists)
            return (false, $"Profile '{createProfileDto.ProfileName}' already exists for this user.", null!);

        // Create new profile entity
        var profile = new UserProfile
        {
            UserId = userId,
            ProfileName = createProfileDto.ProfileName,

        };

        var result = await _repo.Add(profile);

        if (!result.Item1)
            return (false, "Failed to create profile", null!);

        // Map and return saved profile
        var savedProfileDto = new CreateProfileDto
        {
            ProfileName = profile.ProfileName
        };

        return (true, "Profile created successfully", savedProfileDto);
    }


    public async Task<(bool Status, string Message)> UpdateProfileAsync(Guid userId, Guid profileId)
    {
        ApplicationUser? user = await _userService.GetUserByIdentityAsync(userId.ToString());

        if (user is null)
            return (false, "User Not Found");

        UserProfile? profile = user.Profiles.FirstOrDefault(x => x.Id == profileId);

        if (profile is null)
            return (false, "Profile Not Found");

        var result = await _repo.Update(profile);

        if (result == true)
            return (true, "Success");

        else
            return (false, $"Could Not Update Profile For User With Id {userId}");
    }

    public async Task<(bool Status, string Message)> DeleteProfileByUserId(Guid profileId, Guid userId)
    {
        var user = await _userService.GetUserByIdentityAsync(userId.ToString());

        if (user == null)
            return (false, "User not found");

        if (user.Profiles == null || !user.Profiles.Any(x => x.Id == profileId))
            return (false, "Profile not found");

        var result = await _repo.Delete(profileId);

        if (result)
            return (true, "Profile deleted successfully");

        return (false, $"Failed to delete profile with Id: {profileId}");
    }




    public async Task<(bool Status, string Message, IEnumerable<UserHistoryDto> userHistoryListDto)> GetViewingHistoryAsync(Guid userId, Guid profileId)
    {
        ApplicationUser? user = await _userService.GetUserByIdWithProfilesWithHistoriesAsync(userId.ToString());

        if (user is null)
            return (false, "User Not Found", null!);

        UserProfile? profile = user.Profiles.FirstOrDefault(x => x.Id == profileId);

        if (profile is null)
            return (false, "Profile Not Found", null!);

        List<UserHistory>? userHistory = profile.Histories.ToList();

        if (userHistory is null)
            return (false, "History Not Found", null!);

        List<UserHistoryDto> userHistoryListDto = _mapper.Map<List<UserHistoryDto>>(userHistory);

        return (true, "Success", userHistoryListDto);
    }

    // New: Transfer profile ownership to another user by email
    public async Task<(bool Status, string Message)> TransferProfileToUserAsync(Guid profileId, string targetEmail, Guid callerUserId)
    {
        try
        {
            if (profileId == Guid.Empty)
                return (false, "Invalid profile id.");

            if (string.IsNullOrWhiteSpace(targetEmail))
                return (false, "Target email is required.");

            // 1) Load profile
            var profile = await _repo.GetById(profileId);
            if (profile == null)
                return (false, "Profile not found.");

            // 2) Ensure caller is owner of the profile
            if (profile.UserId != callerUserId)
                return (false, "Caller is not the owner of the profile (not authorized).");

            // 3) Resolve target user by email
            var targetUser = await _userService.GetUserByEmailAsync(targetEmail);
            if (targetUser == null)
                return (false, "Target user not found.");

            // 4) Ensure target does not already have any assigned profile
            var targetHasProfiles = targetUser.Profiles != null && targetUser.Profiles.Any();
            if (targetHasProfiles)
                return (false, "Target account already has an assigned profile. Transfer aborted to avoid overwriting.");

            // 5) Ensure profile isn't already assigned to another active user (safety)
            if (profile.UserId != callerUserId)
                return (false, "Profile is assigned to another user.");

            // 6) Transfer ownership
            profile.UserId = targetUser.Id;
            profile.UpdatedDate = DateTime.UtcNow;

            var updated = await _repo.Update(profile);
            if (!updated)
            {
                _logger.LogWarning("Failed to persist profile ownership change for ProfileId={ProfileId}", profileId);
                return (false, "Failed to transfer profile. Please try again.");
            }

            _logger.LogInformation("Profile {ProfileId} transferred from User {SourceUserId} to User {TargetUserId}",
                profileId, callerUserId, targetUser.Id);

            return (true, "Profile transferred successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring profile {ProfileId} by user {CallerId} to email {Email}", profileId, callerUserId, targetEmail);
            return (false, "Internal server error while transferring profile.");
        }
    }

}