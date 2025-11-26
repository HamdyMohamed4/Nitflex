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
        private readonly ILogger<UserService> _logger;
        private readonly IProfileService _profileService;

        public UserService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor accessor,
            IOtpRepository otpRepo,
            TokenService tokenService,
            ILogger<UserService> logger,
            IProfileService profileService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = accessor;
            _otpRepo = otpRepo;
            _tokenService = tokenService;
            _logger = logger;
            _profileService = profileService;
        }

        // New: Transfer profile data from sourceUserId to targetUserId.
        // Validations:
        //  - caller must be the owner of sourceUserId
        //  - target user must exist
        //  - target must not already have profile data (avoid overwriting)
        // Returns tuple (Success, Message)
        public async Task<(bool Success, string Message)> TransferProfileAsync(Guid sourceUserId, Guid targetUserId)
        {
            // Validate input
            if (sourceUserId == Guid.Empty || targetUserId == Guid.Empty)
                return (false, "Source or target user id is invalid.");

            // Ensure caller owns the source account
            Guid callerId;
            try
            {
                callerId = GetLoggedInUser();
            }
            catch
            {
                return (false, "Not authenticated.");
            }

            if (callerId != sourceUserId)
                return (false, "Caller is not the owner of the source profile (not authorized).");

            // Load source & target users
            var sourceUser = await _userManager.FindByIdAsync(sourceUserId.ToString());
            if (sourceUser == null)
                return (false, "Source user not found.");

            var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
            if (targetUser == null)
                return (false, "Target user not found.");

            // Check target does not already have profile data (we treat FullName / PhoneNumber / Profiles as indicators)
            var targetHasProfileData =
                !string.IsNullOrWhiteSpace(targetUser.FullName) ||
                !string.IsNullOrWhiteSpace(targetUser.PhoneNumber) ||
                (targetUser.Profiles != null && targetUser.Profiles.Any());

            // Additionally check for Address or ProfileImage properties if present (do not overwrite)
            bool targetHasAddressOrImage = false;
            var targetType = targetUser.GetType();
            var addrProp = targetType.GetProperty("Address");
            if (addrProp != null)
            {
                var addrVal = addrProp.GetValue(targetUser) as string;
                if (!string.IsNullOrWhiteSpace(addrVal)) targetHasAddressOrImage = true;
            }
            var imgProp = targetType.GetProperty("ProfileImage");
            if (imgProp != null)
            {
                var imgVal = imgProp.GetValue(targetUser) as string;
                if (!string.IsNullOrWhiteSpace(imgVal)) targetHasAddressOrImage = true;
            }

            if (targetHasProfileData || targetHasAddressOrImage)
                return (false, "Target account already has profile data and cannot be overwritten.");

            // Prepare copy: only copy fields that target currently lacks and source has.
            var changedTarget = false;
            var changedSource = false;

            // Email: be cautious - only copy if target email empty and source email present
            if (string.IsNullOrWhiteSpace(targetUser.Email) && !string.IsNullOrWhiteSpace(sourceUser.Email))
            {
                targetUser.Email = sourceUser.Email;
                targetUser.UserName = sourceUser.Email; // keep username aligned
                changedTarget = true;

                sourceUser.Email = null;
                sourceUser.UserName = sourceUser.Id.ToString(); // avoid empty username collision - set to guid
                changedSource = true;
            }

            // FullName
            if (string.IsNullOrWhiteSpace(targetUser.FullName) && !string.IsNullOrWhiteSpace(sourceUser.FullName))
            {
                targetUser.FullName = sourceUser.FullName;
                changedTarget = true;

                sourceUser.FullName = null;
                changedSource = true;
            }

            // PhoneNumber
            if (string.IsNullOrWhiteSpace(targetUser.PhoneNumber) && !string.IsNullOrWhiteSpace(sourceUser.PhoneNumber))
            {
                targetUser.PhoneNumber = sourceUser.PhoneNumber;
                changedTarget = true;

                sourceUser.PhoneNumber = null;
                changedSource = true;
            }

            // Optional Address & ProfileImage (reflection)
            if (addrProp != null)
            {
                var sourceAddr = addrProp.GetValue(sourceUser) as string;
                var targetAddr = addrProp.GetValue(targetUser) as string;
                if (string.IsNullOrWhiteSpace(targetAddr) && !string.IsNullOrWhiteSpace(sourceAddr))
                {
                    addrProp.SetValue(targetUser, sourceAddr);
                    addrProp.SetValue(sourceUser, null);
                    changedTarget = true;
                    changedSource = true;
                }
            }

            if (imgProp != null)
            {
                var sourceImg = imgProp.GetValue(sourceUser) as string;
                var targetImg = imgProp.GetValue(targetUser) as string;
                if (string.IsNullOrWhiteSpace(targetImg) && !string.IsNullOrWhiteSpace(sourceImg))
                {
                    imgProp.SetValue(targetUser, sourceImg);
                    imgProp.SetValue(sourceUser, null);
                    changedTarget = true;
                    changedSource = true;
                }
            }

            // Persist changes: update both users with Identity
            if (changedTarget)
            {
                var updateTargetResult = await _userManager.UpdateAsync(targetUser);
                if (!updateTargetResult.Succeeded)
                {
                    var errors = string.Join(", ", updateTargetResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to update target user {TargetId} during profile transfer: {Errors}", targetUser.Id, errors);
                    return (false, "Failed to update the target user account.");
                }
            }

            if (changedSource)
            {
                var updateSourceResult = await _userManager.UpdateAsync(sourceUser);
                if (!updateSourceResult.Succeeded)
                {
                    var errors = string.Join(", ", updateSourceResult.Errors.Select(e => e.Description));
                    _logger.LogWarning("Failed to update source user {SourceId} during profile transfer: {Errors}", sourceUser.Id, errors);
                    // Attempt to rollback target update if possible: we won't attempt deep rollback here, but log and notify.
                    return (false, "Failed to update the source user account after transfer. Contact support.");
                }
            }

            _logger.LogInformation("User {SourceId} transferred profile data to user {TargetId}", sourceUser.Id, targetUser.Id);

            return (true, "Profile transferred successfully.");
        }

        // Implement TransferProfileByEmailAsync by delegating to ProfileService (clean separation)
        public async Task<(bool Success, string Message)> TransferProfileByEmailAsync(Guid sourceUserId, Guid profileId, string targetEmail)
        {
            try
            {
                if (sourceUserId == Guid.Empty) return (false, "Source user id is invalid.");
                if (profileId == Guid.Empty) return (false, "Profile id is invalid.");
                if (string.IsNullOrWhiteSpace(targetEmail)) return (false, "Target email is required.");

                // Ensure caller is the logged-in user
                Guid callerId;
                try
                {
                    callerId = GetLoggedInUser();
                }
                catch
                {
                    return (false, "Not authenticated.");
                }

                if (callerId != sourceUserId)
                    return (false, "Caller is not the owner of the profile (not authorized).");

                var (status, message) = await _profileService.TransferProfileToUserAsync(profileId, targetEmail, sourceUserId);
                return (status, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TransferProfileByEmailAsync: source={Source}, profile={ProfileId}, email={Email}", sourceUserId, profileId, targetEmail);
                return (false, "Internal server error while transferring profile.");
            }
        }



        public async Task<bool> ResetPasswordAsync(string email, string code, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            // Validate OTP
            var isValidOtp = await _otpRepo.ValidateOtpAsync(email, code);
            if (!isValidOtp) return false;

            token = Uri.UnescapeDataString(token);

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                // Mark OTP as used
                await _otpRepo.MarkOtpAsUsedAsync(email, code);
                return true;
            }

            return false;       
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

        //public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        //{
        //    var user = await _userManager.FindByEmailAsync(email);

        //    if (user == null)
        //        return null;

        //    if (user.IsBlocked)
        //        return null;

        //    return user;
        //}


        public async Task<(bool IsValid, string ErrorMessage, ApplicationUser? User)> ValidateLoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return (false, "User Is Not Exist", null);

            if (user.IsBlocked)
                return (false, "User is blocked", null);

            //if (!user.EmailConfirmed)
            //    return (false, "Email is not verified", null);

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);

            if (!passwordValid)
                return (false, "Invalid credentials", null);

            return (true, string.Empty, user);
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }




        public async Task<LoginDto?> GetUserByLoginAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            if (user.IsBlocked == true)
                return null;

            return new LoginDto
            {
                Email = user.Email ?? string.Empty,
                Password = string.Empty,
            };
        }

        public async Task<ApplicationUser?> GetUserByEmailAsyncs(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            if (user.IsBlocked == true)
                return null;

            return new ApplicationUser
            {
                Email = user.Email ?? string.Empty,
            };
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

        public async Task<LoginResponseDto?> LoginWithOtpAsync(string email, string otp)
        {
            var otpRecord = await _otpRepo.GetValidOtpAsync(email, otp);
            if (otpRecord == null)
                return null;

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            // Mark OTP as used
            await _otpRepo.MarkOtpAsUsedAsync(email, otp);

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

        public async Task<ApplicationUser?> GetUserByIdWithProfilesAsync()
        {
            return await _userManager.Users.Include(x => x.Profiles).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<ApplicationUser?> GetUserByIdWithProfilesWithHistoriesAsync(string userId)
        {
            return await _userManager.Users.Include(x => x.Profiles).ThenInclude(x => x.Histories).AsNoTracking().FirstOrDefaultAsync(x => x.Id.ToString() == userId);
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            var otp = await _otpRepo.GetValidOtpAsync(email, token);
            if (otp == null)
                return false;

            // generate a real Identity token
            var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            var resetResult = await _userManager.ResetPasswordAsync(user, identityToken, newPassword);
            if (!resetResult.Succeeded)
                return false;

            await _otpRepo.MarkOtpAsUsedAsync(email, token);

            return true;
        }



        public async Task<ApplicationUser?> GetUserByIdWithProfilesAsync(Guid userId)
        {
            return await _userManager.Users
                .Include(u => u.Profiles)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }




    }
}