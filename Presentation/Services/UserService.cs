using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Presentation.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOtpRepository _otpRepo;
        private readonly TokenService _tokenService;
        private readonly IRefreshTokens _refreshTokenService;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
             IHttpContextAccessor accessor, IOtpRepository otpRepo, TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = accessor;
            _otpRepo = otpRepo;
            _tokenService = tokenService;
        }


        public async Task<LoginResponseDto?> ConfirmSignUpAsync(string email, string token)
        {
            // 1. تحقق من صحة التوكن
            var otpRecord = await _otpRepo.GetValidOtpAsync(email, token);
            if (otpRecord == null)
                return null; // توكن غير صالح أو منتهي

            // 2. تحقق إذا كان المستخدم موجودًا في قاعدة البيانات
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // إذا لم يكن موجودًا، قم بإنشاء مستخدم جديد
                user = new ApplicationUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                    return null; // إذا حدث خطأ أثناء الإنشاء

                await _userManager.AddToRoleAsync(user, "User");
            }

            // 3. تأكيد البريد الإلكتروني إذا لم يكن تم تأكيده بعد
            if (!user.EmailConfirmed)
            {
                var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
                if (!confirmResult.Succeeded)
                    return null; // إذا فشل التأكيد
            }

            // 4. توليد التوكنات
            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // 5. حفظ الـ Refresh Token في قاعدة البيانات
            await _refreshTokenService.Refresh(new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = user.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            });

            // 6. إرجاع التوكنات للمستخدم
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = user.Id.ToString(),
                Email = user.Email
            };
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

            return new RegisterDto
            {
                Email = user.Email ?? string.Empty,
                Password = string.Empty,
            };
        }

        public async Task<ApplicationUser?> GetUserByEmailAsyncs(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<RegisterDto?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            return new RegisterDto { Email = user.Email ?? string.Empty, Password = string.Empty };
        }

        public async Task<IEnumerable<RegisterDto>> GetAllUsersAsync()
        {
            return _userManager.Users.Select(u => new RegisterDto
            {
                Email = u.Email ?? string.Empty,
                Password = string.Empty,
            }).ToList();
        }


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
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = false };
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
            await _otpRepo.MarkOtpUsedAsync(email, otp);

            return new LoginResponseDto
            {
                UserId = user.Id.ToString(),
                Email = user.Email,
            };
        }


        public async Task<IList<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            return await _userManager.GetRolesAsync(user);
        }

        public async Task<ApplicationUser?> GetUserByIdWithProfilesAsync(string userId)
        {
            return await _userManager.Users.Include(x => x.Profiles).AsNoTracking().FirstOrDefaultAsync(x => x.Id.ToString() == userId);
        }

        public async Task<ApplicationUser?> GetUserByIdWithProfilesWithHistoriesAsync(string userId)
        {
            return await _userManager.Users.Include(x => x.Profiles).ThenInclude(x => x.Histories).AsNoTracking().FirstOrDefaultAsync(x => x.Id.ToString() == userId);
        }


        //public async Task<(bool Success, string? Id)> CreateUserWithoutPasswordAndGetTokenAsync(string email)
        //{
        //    var user = new ApplicationUser { Email = email, UserName = email };
        //    var result = await _userManager.CreateAsync(user);
        //    if (!result.Succeeded) return (false, null);

        //    // Generate token (مثلاً email confirmation token)
        //    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    return (true, token);
        //}


    }
}