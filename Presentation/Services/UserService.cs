using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
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
        private readonly IOtpRepository _otpRepo;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
             IHttpContextAccessor accessor,IOtpRepository otpRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = accessor;
            _otpRepo = otpRepo;
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

        //public Guid GetLoggedInUser()
        //{
        //    var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;
        //    if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
        //        throw new InvalidOperationException("No valid logged-in user ID found in claims.");
        //    return userId;
        //}


        public Guid GetLoggedInUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null)
                throw new InvalidOperationException("No HttpContext or User found.");

            // محاولة الحصول على الـ ID من أكثر من Claim
            var userIdClaim =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                user.FindFirst("sub")?.Value ??
                user.FindFirst("uid")?.Value ??
                user.FindFirst("id")?.Value ??
                user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                throw new InvalidOperationException("No valid logged-in user ID found in claims.");

            return Guid.Parse(userIdClaim);
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

        //public async Task<LoginResponseDto> LoginWithOtpAsync(string email, string code)
        //{
        //    // تحقق من الكود
        //    var otpValid = await _otpRepo.GetValidOtpAsync(email, code);
        //    if (otpValid == null || otpValid.ExpirationDate < DateTime.UtcNow)
        //        return null!; // أو throw exception أو ترجع null

        //    // علامة استخدام الكود
        //    await _otpRepo.MarkOtpUsedAsync(email, code);

        //    // تحقق من وجود المستخدم
        //    var user = await _userManager.FindByEmailAsync(email);
        //    if (user == null)
        //    {
        //        user = new ApplicationUser
        //        {
        //            UserName = email,
        //            Email = email,
        //            Name = email.Split('@')[0],
        //            EmailConfirmed = true
        //        };
        //        var result = await _userManager.CreateAsync(user);
        //        if (!result.Succeeded) return null!;
        //        await _userManager.AddToRoleAsync(user, "User");
        //    }

        //    // هنا تحول الـ UserResultDto إلى LoginResponseDto
        //    var loginResponse = new LoginResponseDto
        //    {
        //        AccessToken = "DEMO_ACCESS_TOKEN",   // هنا تحط منطق إنشاء JWT فعلي
        //        RefreshToken = "DEMO_REFRESH_TOKEN",
        //        UserId = user.Id.ToString()
        //    };

        //    return loginResponse;
        //}

        public async Task<LoginResponseDto?> LoginWithOtpAsync(string email, string otp)
        {
            var otpRecord = await _otpRepo.GetValidOtpAsync(email, otp);
            if (otpRecord == null)
                return null;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            // Mark OTP as used
            await _otpRepo.MarkOtpUsedAsync(email,otp);

            return new LoginResponseDto
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
            };
        }


        // ---- الميثود الجديدة ----
        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);

            return roles.ToList();
        }




    }
}


