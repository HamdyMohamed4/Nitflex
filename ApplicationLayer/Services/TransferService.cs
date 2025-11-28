using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Domains;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.UserModels;
using Microsoft.Extensions.Logging;
using Presentation.Services;

namespace ApplicationLayer.Services
{
    public class TransferService : ITransferService
    {
        private readonly ITransferRequestRepository _transferRepo;
        private readonly IUserService _userService;
        private readonly IGenericRepository<UserProfile> _profileRepo;
        private readonly EmailService _emailService; // use existing email service
        private readonly ILogger<TransferService> _logger;
        private readonly TimeSpan _requestTtl = TimeSpan.FromDays(2);

        public TransferService(
            ITransferRequestRepository transferRepo,
            IUserService userService,
            IGenericRepository<UserProfile> profileRepo,
            EmailService emailService,
            ILogger<TransferService> logger)
        {
            _transferRepo = transferRepo;
            _userService = userService;
            _profileRepo = profileRepo;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> InitiateTransferAsync(Guid callerUserId, InitiateProfileTransferRequest dto, string frontendConfirmationUrlBase)
        {
            if (dto.ProfileId == Guid.Empty) return (false, "ProfileId is required.");
            if (string.IsNullOrWhiteSpace(dto.TargetEmail)) return (false, "Target email is required.");

            // validate profile exists and belongs to caller
            var profile = await _profileRepo.GetById(dto.ProfileId);
            if (profile == null) return (false, "Profile not found.");

            if (profile.UserId != callerUserId)
                return (false, "You are not the owner of the profile.");

            // validate target user exists
            var targetUser = await _userService.GetUserByEmailAsync(dto.TargetEmail.Trim());
            if (targetUser == null) return (false, "Target user not found.");

            // validate target does not already have a profile
            if (targetUser.Profiles != null && targetUser.Profiles.Any())
                return (false, "Target user already has an assigned profile.");

            // ensure profile isn't currently assigned to another active user - double check
            if (profile.UserId != callerUserId)
                return (false, "Profile is assigned to another user.");

            // create transfer request with secure token
            var token = Guid.NewGuid().ToString("N");
            var tr = new TransferRequest
            {
                ProfileId = profile.Id,
                SourceUserId = callerUserId,
                TargetUserId = targetUser.Id,
                TargetEmail = dto.TargetEmail.Trim(),
                Token = token,
                ExpiresAt = DateTime.UtcNow.Add(_requestTtl),
                Status = TransferRequestStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            await _transferRepo.AddAsync(tr);

            // compose confirmation link (frontend URL base is provided by caller or from config)
            // e.g. frontendConfirmationUrlBase = "https://app.example.com/transfer/confirm"
            var confirmLink = $"{frontendConfirmationUrlBase.TrimEnd('/')}/api/UserProfile/transfer/confirm?token={token}";

            // send email to target user
            var subject = "Profile transfer request";
            var body = $@"
                <p>Hello,</p>
                <p>User with email <strong>{(await _userService.GetUserByIdentityAsync(callerUserId.ToString()))?.Email}</strong> has requested to transfer a profile to your account.</p>
                <p>Please <a href='{confirmLink}'>click here</a> to review and approve the transfer. This link expires on {tr.ExpiresAt:u}.</p>
                <p>If you don't want to accept, you may ignore this email or use the reject option on the confirmation page.</p>
            ";
            try
            {
                await _emailService.SendEmailAsync(tr.TargetEmail, subject, body);
                _logger.LogInformation("Transfer request created (Token={Token}) from {Source} to {TargetEmail}", token, callerUserId, tr.TargetEmail);
                return (true, "Transfer request initiated. Confirmation email sent to target user.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send transfer confirmation email for Token={Token}", token);
                return (false, "Failed to send confirmation email.");
            }
        }

        public async Task<(bool Success, string Message)> CompleteTransferAsync(ConfirmTransferRequest dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token)) return (false, "Token is required.");
            var tr = await _transferRepo.GetByTokenAsync(dto.Token.Trim());
            if (tr == null) return (false, "Transfer request not found.");

            if (tr.Status != TransferRequestStatus.Pending) return (false, "Transfer request is not pending.");

            if (tr.ExpiresAt < DateTime.UtcNow)
            {
                tr.Status = TransferRequestStatus.Expired;
                tr.ProcessedAt = DateTime.UtcNow;
                await _transferRepo.UpdateAsync(tr);
                return (false, "Transfer request has expired.");
            }

            // Fetch latest profile and target user
            var profile = await _profileRepo.GetById(tr.ProfileId);
            if (profile == null) return (false, "Profile not found.");

            var targetUser = await _userService.GetUserByIdentityAsync(tr.TargetUserId.ToString());
            if (targetUser == null) return (false, "Target user not found.");

            // Ensure target still has no profile
            if (targetUser.Profiles != null && targetUser.Profiles.Any())
            {
                tr.Status = TransferRequestStatus.Rejected;
                tr.ProcessedAt = DateTime.UtcNow;
                await _transferRepo.UpdateAsync(tr);
                return (false, "Target user already has profile(s). Transfer rejected.");
            }

            if (!dto.Approve)
            {
                tr.Status = TransferRequestStatus.Rejected;
                tr.ProcessedAt = DateTime.UtcNow;
                await _transferRepo.UpdateAsync(tr);
                _logger.LogInformation("Transfer request rejected (Token={Token})", dto.Token);
                return (true, "Transfer rejected.");
            }

            // perform ownership change
            profile.UserId = tr.TargetUserId;
            profile.UpdatedDate = DateTime.UtcNow;
            var updated = await _profileRepo.Update(profile);
            if (!updated)
            {
                _logger.LogWarning("Failed to update profile ownership for ProfileId={ProfileId}", profile.Id);
                return (false, "Failed to change profile ownership.");
            }

            tr.Status = TransferRequestStatus.Approved;
            tr.ProcessedAt = DateTime.UtcNow;
            await _transferRepo.UpdateAsync(tr);

            _logger.LogInformation("Transfer request approved (Token={Token}) profile {ProfileId} -> user {TargetUserId}",
                dto.Token, profile.Id, tr.TargetUserId);

            return (true, "Profile transfer approved and completed.");
        }
    }
}
