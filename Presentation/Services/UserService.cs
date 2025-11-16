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

            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description)
                };
            }

            // Assign a default role "User"
            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if (!roleResult.Succeeded)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = roleResult.Errors.Select(e => e.Description)
                };
            }

            return new UserResultDto
            {
                Success = true
            };
        }

        public async Task<UserResultDto> LoginAsync(LoginDto loginDto)
        {
            var result = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, false, false);

            if (!result.Succeeded)
            {
                return new UserResultDto
                {
                    Success = false,
                    Errors = new[] { "Invalid login attempt." }
                };
            }

            return new UserResultDto
            {
                Success = true,
                Token = "JWT_WILL_BE_GENERATED"
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<RegisterDto> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null) return null;

            return new RegisterDto
            {
                Id = user.Id,
                Email = user.Email,

            };
        }

        public async Task<RegisterDto> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null) return null;

            return new RegisterDto
            {
                Id = user.Id,
                Email = user.Email,
            };
        }

        public async Task<IEnumerable<RegisterDto>> GetAllUsersAsync()
        {
            return _userManager.Users.Select(u => new RegisterDto
            {
                Id = u.Id,
                Email = u.Email,
            });
        }

        public Guid GetLoggedInUser()
        {
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdStr))
                throw new InvalidOperationException("No logged-in user found");

            return Guid.Parse(userIdStr);
        }


        //public Guid GetLoggedInUser()
        //{
        //    var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        //    return Guid.Parse(userId);
        //}
    }
}
