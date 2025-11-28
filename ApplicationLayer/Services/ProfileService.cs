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
    private readonly IPasswordHasher<UserProfile> _pinHasher;

    public ProfileService(IProfileRepository profileRepository, IMapper mapper, ILogger<ProfileService> logger, UserManager<ApplicationUser> userManager)
    {
        _profileRepository = profileRepository;
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _pinHasher = new PasswordHasher<UserProfile>();
    }

    public async Task<UserProfileDto> CreateProfileAsync(CreateProfileDto dto, Guid userId)
    {
        var count = await _profileRepository.CountByUserIdAsync(userId);
        if (count >= MaxProfilesPerUser)
            throw new InvalidOperationException("User already has maximum number of profiles");

        var existingProfiles = await _profileRepository.GetAllByUserIdAsync(userId);
        if (existingProfiles.Any(p => p.ProfileName.Equals(dto.ProfileName, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Profile name already exists for this user");

        var profile = new UserProfile
        {
            ProfileName = dto.ProfileName,
            UserId = userId,
            CurrentState = 1,
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
        var target = await _userManager.FindByEmailAsync(targetEmail.Trim());
        if (target == null) return (false, "Target user not found.");

        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null) return (false, "Profile not found.");

        if (profile.UserId != callerUserId) return (false, "Caller is not owner of the profile.");

        var targetCount = await _profileRepository.CountByUserIdAsync(target.Id);
        if (targetCount > 0) return (false, "Target already has profile(s).");

        var deleted = await _profileRepository.DeleteAsync(profileId, callerUserId);
        if (!deleted) return (false, "Failed to detach profile from source user.");

        profile.Id = Guid.NewGuid();
        profile.UserId = target.Id;
        await _profileRepository.AddAsync(profile);

        return (true, "Profile transferred successfully.");
    }

    public async Task<bool> LockProfileAsync(Guid profileId, Guid userId, string pin)
    {
        if (string.IsNullOrEmpty(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            throw new ArgumentException("PIN must be exactly 4 numeric digits.");

        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null) return false;
        if (profile.UserId != userId) throw new UnauthorizedAccessException("Not owner of profile.");

        profile.IsLocked = true;
        profile.PinHash = _pinHasher.HashPassword(profile, pin);

        // update via repository - simpler to delete+add not ideal; add UpdateAsync to repository would be better
        // For now use NetflixContext via repository if available. We'll extend repository with UpdateAsync.
        if (await TryUpdateProfileAsync(profile)) return true;

        return false;
    }

    public async Task<bool> UnlockProfileAsync(Guid profileId, Guid userId, string pin)
    {
        if (string.IsNullOrEmpty(pin) || pin.Length != 4 || !pin.All(char.IsDigit))
            throw new ArgumentException("PIN must be exactly 4 numeric digits.");

        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null) return false;
        if (profile.UserId != userId) throw new UnauthorizedAccessException("Not owner of profile.");

        if (!profile.IsLocked || string.IsNullOrEmpty(profile.PinHash)) return false;

        var verification = _pinHasher.VerifyHashedPassword(profile, profile.PinHash, pin);
        if (verification == PasswordVerificationResult.Success || verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            profile.IsLocked = false;
            profile.PinHash = null;
            if (verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                // Rehash to latest format
                profile.PinHash = _pinHasher.HashPassword(profile, pin);
            }

            if (await TryUpdateProfileAsync(profile)) return true;
        }

        return false;
    }

    public async Task<bool> ToggleKidModeAsync(Guid profileId, Guid userId, bool enable)
    {
        var profile = await _profileRepository.GetByIdAsync(profileId);
        if (profile == null) return false;
        if (profile.UserId != userId) throw new UnauthorizedAccessException("Not owner of profile.");

        profile.IsKidProfile = enable;
        return await TryUpdateProfileAsync(profile);
    }

    // Helper to update profile via repository. Add UpdateAsync to IProfileRepository and implement in concrete repo.
    private async Task<bool> TryUpdateProfileAsync(UserProfile profile)
    {
        if (_profileRepository is null) return false;

        // If repository exposes Update, use it. We'll attempt a cast to known concrete repo with access to DbContext.
        if (_profileRepository is InfrastructureLayer.Repositories.ProfileRepository concrete)
        {
            return await concrete.UpdateAsync(profile);
        }

        return false;
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