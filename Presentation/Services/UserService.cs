using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Presentation.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
             IHttpContextAccessor accessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = accessor;
        }

        public async Task<UserResultDto> RegisterAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Name = registerDto.Name
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded) return new UserResultDto { Success = false, Errors = result.Errors.Select(e => e.Description) };

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded) return new UserResultDto { Success = false, Errors = roleResult.Errors.Select(e => e.Description) };

            return new UserResultDto { Success = true, Id = user.Id };
        }

        public async Task<UserResultDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null) return new UserResultDto { Success = false, Errors = new[] { "Invalid login attempt." } };

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return new UserResultDto { Success = false, Errors = new[] { "Invalid login attempt." } };

            return new UserResultDto { Success = true, Id = user.Id };
        }

        public async Task LogoutAsync() => await _signInManager.SignOutAsync();

        public async Task<RegisterDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            return new RegisterDto { Id = user.Id, Email = user.Email ?? string.Empty, Password = string.Empty, Name = user.Name ?? string.Empty };
        }

        public async Task<RegisterDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            return new RegisterDto { Id = user.Id, Email = user.Email ?? string.Empty, Password = string.Empty, Name = user.Name ?? string.Empty };
        }

        public async Task<IEnumerable<RegisterDto>> GetAllUsersAsync()
        {
            return _userManager.Users.Select(u => new RegisterDto
            {
                Id = u.Id,
                Email = u.Email ?? string.Empty,
                Password = string.Empty,
                Name = u.Name ?? string.Empty
            }).ToList();
        }

        public Guid GetLoggedInUser()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
                throw new InvalidOperationException("No valid logged-in user ID found in claims.");
            return userId;
        }

        public async Task<ApplicationUser?> GetUserByIdentityAsync(string userId) => await _userManager.FindByIdAsync(userId);

        // --- Magic Link Implementations ---
        public async Task<(UserResultDto Result, string? Token)> CreateUserWithoutPasswordAndGetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, Name = email.Split('@')[0], EmailConfirmed = false };
                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded) return (new UserResultDto { Success = false, Errors = result.Errors.Select(e => e.Description) }, null);
                await _userManager.AddToRoleAsync(user, "User");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return (new UserResultDto { Success = true, Id = user.Id }, token);
        }

        public async Task<UserResultDto> ConfirmEmailAsync(Guid userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new UserResultDto { Success = false, Errors = new[] { "User not found." } };

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded) return new UserResultDto { Success = false, Errors = result.Errors.Select(e => e.Description) };

            await _signInManager.SignInAsync(user, isPersistent: false);

            return new UserResultDto { Success = true };
        }

        public async Task<UserResultDto> SetPasswordAsync(Guid userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new UserResultDto { Success = false, Errors = new[] { "User not found." } };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, password);

            if (!result.Succeeded) return new UserResultDto { Success = false, Errors = result.Errors.Select(e => e.Description) };

            return new UserResultDto { Success = true };
        }
    }
}


