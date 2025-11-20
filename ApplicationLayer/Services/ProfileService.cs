using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Azure;
using Domains;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.UserModels;
using Org.BouncyCastle.Asn1.Misc;

namespace ApplicationLayer.Services;

public class ProfileService : BaseService<UserProfile, UserProfileDto>, IProfileService
{
    public ProfileService(IGenericRepository<UserProfile> repo, IMapper mapper, IUserService userService)
        : base(repo, mapper, userService) { }

    public async Task<(bool Status, string Message, IEnumerable<UserProfileDto> Response)> GetAllProfilesByUserIdAsync(Guid userId)
    {
        ApplicationUser? user = await _userService.GetUserByIdWithProfilesAsync(userId.ToString());

        if (user is null)
            return (false, "User Not Found", null!);

        List<UserProfile> userProfiles = user.Profiles.ToList();

        return (true, "Success", _mapper.Map<List<UserProfileDto>>(userProfiles));
    }

    public async Task<(bool Status, string Message, UserProfileDto? userProfileDto)> GetProfileByUserIdAsync(Guid userId, Guid profileId)
    {
        ApplicationUser? user = await _userService.GetUserByIdWithProfilesAsync(userId.ToString());

        if (user is null)
            return (false, "User Not Found", null!);

        var x = user.Profiles;

        UserProfile? userProfile = user.Profiles.FirstOrDefault(x => x.UserId == userId);

        if (userProfile is null)
            return (false, "Profile Not Found", null!);

        UserProfileDto userProfileDto = _mapper.Map<UserProfileDto>(userProfile);

        return (true, "Success", userProfileDto);
    }

    public async Task<(bool Status, string Message, CreateProfileDto userProfileDto)> CreateProfileAsync(Guid userId, CreateProfileDto createProfileDto)
    {
        ApplicationUser? user = await _userService.GetUserByIdentityAsync(userId.ToString());

        if (user is null)
            return (false, "User Not Found", null!);

        UserProfile userProfile = new UserProfile { UserId = createProfileDto.UserId, ProfileName = createProfileDto.ProfileName };

        var result = await _repo.Add(userProfile);

        if (result.Item1 == true)
            return (true, $"{result.Item2}", createProfileDto);

        else
            return (false, $"Could not created profile for user with Id {userId}", null!);
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
            return (false, $"Could Not Updated Profile For User With Id {userId}");
    }

    public async Task<(bool Status, string Message)> DeleteProfileByUserId(Guid userId, Guid profileId)
    {
        ApplicationUser? user = await _userService.GetUserByIdWithProfilesAsync(userId.ToString());

        if (user is null)
            return (false, "User Not Found");

        if (!user.Profiles.Any(x => x.Id == profileId))
            return (false, "Profile Not Found");

        var result = await _repo.Delete(profileId);

        if (result)
            return (true, "Success");

        return (false, $"Failed to delete profile with Id: {profileId} that belongs to user with id: {userId}");
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
}