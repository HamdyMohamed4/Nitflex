using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using AutoMapper;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public AdminUserService(UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _userManager = userManager;
        _mapper = mapper;
    }

    // ===========================
    // Get all users
    // ===========================
    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = _userManager.Users
                 .Where(u => u.CurrentState == 1)
                 .ToList();

        return _mapper.Map<IEnumerable<UserDto>>(users);
    }


    // ===========================
    // Get user by Id
    // ===========================
    public async Task<UserDto?> GetByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || user.CurrentState == 0)
            return null;

        return _mapper.Map<UserDto>(user);
    }

    // ===========================
    // Create user
    // ===========================
    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {

        var user = _mapper.Map<ApplicationUser>(dto);

        user.Id = Guid.NewGuid();
        user.CurrentState = 1;
        user.IsBlocked = false;
        user.UserName = dto.Email;
        user.EmailConfirmed = true;

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded) return null;

        return _mapper.Map<UserDto>(user);
    }


    // ===========================
    // Update user
    // ===========================
    public async Task<bool> UpdateAsync(Guid userId, UpdateUserDto dto)
    {

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || user.CurrentState == 0) return false;

        user.Email = dto.Email;        
        user.UserName = dto.Email;
        user.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, dto.Role);
        }

       
        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }

    // ===========================
    // Delete user
    // ===========================
    public async Task<bool> DeleteAsync(Guid userId)
    {

        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || user.CurrentState == 0) return false;

        user.CurrentState = 0;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;

    }

    // ===========================
    // Block / Unblock user
    // ===========================
    public async Task<bool> BlockAsync(Guid userId, UserBlockDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null || user.CurrentState == 0) return false;

        user.IsBlocked = dto.Blocked;
        user.BlockReason = dto.Reason;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;

    }
}
