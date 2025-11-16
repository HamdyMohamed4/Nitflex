using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationLayer.Services
{
    public class AuthService : IAuthService
    {
        //private readonly IUserService _userService;

        //public AuthService(IUserService userService)
        //{
        //    _userService = userService;
        //}

        //public async Task<bool> SendMagicLinkAsync(string email)
        //{
        //    var (result, token) = await _userService.CreateUserWithoutPasswordAndGetTokenAsync(email);

        //    if (!result.Success || token == null) return false;

        //    var magicLink = $"[Frontend_Base_URL]/auth/confirm-signup?userId={result.Id}&token={Uri.EscapeDataString(token)}";
        //    Console.WriteLine($"[DEMO] Magic Link for {email}: {magicLink}");

        //    return true;
        //}

        //public async Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token)
        //{
        //    if (!Guid.TryParse(userId, out Guid userGuid)) return null;

        //    var confirmResult = await _userService.ConfirmEmailAsync(userGuid, token);
        //    if (!confirmResult.Success) return null;

        //    var user = await _userService.GetUserByIdentityAsync(userId);
        //    if (user == null) return null;

        //    // --- TOKEN GENERATION ---
        //    return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", user.Id);
        //}

        //public async Task<LoginResponseDto?> RegisterWithPasswordAsync(RegisterDto model)
        //{
        //    var registerResult = await _userService.RegisterAsync(model);

        //    if (!registerResult.Success) return null;

        //    return await LoginAsync(model.Email, model.Password);
        //}

        //public async Task<LoginResponseDto?> LoginAsync(string email, string password)
        //{
        //    var loginDto = new LoginDto(email, password);
        //    var loginResult = await _userService.LoginAsync(loginDto);

        //    if (!loginResult.Success) return null;

        //    var userDto = await _userService.GetUserByEmailAsync(email);
        //    if (userDto == null) return null;

        //    // --- TOKEN GENERATION ---
        //    return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", userDto.Id);
        //}


        private readonly IUserService _userService;
        private readonly EmailService _emailService;

        public AuthService(IUserService userService, EmailService emailService)
        {
            _userService = userService;
            _emailService = emailService;
        }

        public async Task<bool> SendMagicLinkAsync(string email)
        {
            var (result, token) = await _userService.CreateUserWithoutPasswordAndGetTokenAsync(email);

            if (!result.Success || token == null) return false;

            var magicLink = $"https://localhost:7263/api/auth/confirm-signup?userId={result.Id}&token={Uri.EscapeDataString(token)}";

            string emailBody = $@"
            <h3>Welcome to WatchMe!</h3>
            <p>Click the link below to complete your registration:</p>
            <a href='{magicLink}'>Confirm Registration</a>
            <p>If you did not request this, ignore this email.</p>";

            await _emailService.SendEmailAsync(email, "WatchMe Sign-Up Confirmation", emailBody);

            return true;
        }

    // باقي الـ AuthService بدون تغيير


        public async Task<LoginResponseDto?> RegisterUserFromMagicLinkAsync(string userId, string token)
        {
            if (!Guid.TryParse(userId, out Guid userGuid)) return null;

            var confirmResult = await _userService.ConfirmEmailAsync(userGuid, token);
            if (!confirmResult.Success) return null;

            var user = await _userService.GetUserByIdentityAsync(userId);
            if (user == null) return null;

            // TODO: Generate real JWT here
            return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", user.Id);
        }

        public async Task<LoginResponseDto?> RegisterWithPasswordAsync(RegisterDto model)
        {
            var registerResult = await _userService.RegisterAsync(model);
            if (!registerResult.Success) return null;

            return await LoginAsync(model.Email, model.Password);
        }

        public async Task<LoginResponseDto?> LoginAsync(string email, string password)
        {
            var loginResult = await _userService.LoginAsync(new LoginDto(email, password));
            if (!loginResult.Success) return null;

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return null;

            // TODO: Generate real JWT here
            return new LoginResponseDto("DEMO_JWT_TOKEN", "DEMO_REFRESH_TOKEN", user.Id);
        }
    }
}
