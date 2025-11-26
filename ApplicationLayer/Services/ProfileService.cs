using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ApplicationLayer.Services;

public class ProfileService : IProfileService
{
    private const int MaxProfilesPerUser = 5;
    private readonly IProfileRepository _profileRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProfileService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileService(IProfileRepository profileRepository, IMapper mapper, ILogger<ProfileService> logger, UserManager<ApplicationUser> userManager)
    {
        _profileRepository = profileRepository;
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<UserProfileDto> CreateProfileAsync(CreateProfileDto dto, Guid userId)
    {
        var count = await _profileRepository.CountByUserIdAsync(userId);
        if (count >= MaxProfilesPerUser)
            throw new InvalidOperationException("User already has maximum number of profiles");

        var profile = new UserProfile
        {
            ProfileName = dto.ProfileName,
            UserId = userId,
            CurrentState=1, 
        };

        var added = await _profileRepository.AddAsync(profile);
        return _mapper.Map<UserProfileDto>(added);
    }

    public async Task<List<UserProfileDto>> GetAllProfilesAsync(Guid userId)
    {
        var list = await _profileRepository.GetAllByUserIdAsync(userId);
        return _mapper.Map<List<UserProfileDto>>(list);
    }

    public async Task<UserProfileDto?> GetProfileByIdAsync(Guid profileId, Guid userId)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null || profile.UserId != userId) return null;
        return _mapper.Map<UserProfileDto>(profile);
    }

    public async Task<bool> DeleteProfileAsync(Guid profileId, Guid userId)
    {
        return await _profileRepository.DeleteAsync(profileId, userId);
    }

    public async Task<(bool Status, string Message)> TransferProfileToUserAsync(Guid profileId, string targetEmail, Guid callerUserId)
    {
        // Basic implementation used by Presentation.Services.UserService.TransferProfileByEmailAsync
        var target = await _userManager.FindByEmailAsync(targetEmail.Trim());
        if (target == null) return (false, "Target user not found.");

        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null) return (false, "Profile not found.");

        if (profile.UserId != callerUserId) return (false, "Caller is not owner of the profile.");

        // Ensure target has no profiles
        var targetCount = await _profileRepository.CountByUserIdAsync(target.Id);
        if (targetCount > 0) return (false, "Target already has profile(s).");

        // Change ownership
        profile.UserId = target.Id;
        // Use UnitOfWork generic repository is not available here; call delete/add via repository or update via DbContext.
        // For simplicity, we update via repository by deleting and re-adding.
        // Better approach: add Update method to IProfileRepository. For now, remove and add new.

        // Directly update via context would be simpler but keep repository boundary - assume ProfileRepository can handle update through EF.
        // Attempt to update by re-adding Id and saving using GenericRepository if exists (not ideal)

        // This is a simple approach: delete then add
        var deleted = await _profileRepository.DeleteAsync(profileId, callerUserId);
        if (!deleted) return (false, "Failed to detach profile from source user.");

        profile.Id = Guid.NewGuid();
        profile.UserId = target.Id;
        await _profileRepository.AddAsync(profile);

        return (true, "Profile transferred successfully.");
    }

    // Implement IBaseService minimal members to satisfy interface; throw NotImplemented for unused methods
    public Task<List<UserProfileDto>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<UserProfileDto> GetById(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<(bool, Guid)> Add(UserProfileDto entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> Update(UserProfileDto entity)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ChangeStatus(Guid id, int status = 1)
    {
        throw new NotImplementedException();
    }
}