using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IOtpRepository
    {
        //// حفظ OTP جديد
        //Task SaveOtpAsync(string userId, string code, DateTime expires);

        //// التحقق من صلاحية OTP (صالح ولم يُستخدم)
        //Task<bool> ValidateOtpAsync(string userId, string code);

        //// وضع OTP كمُستخدم (used)
        //Task MarkOtpUsedAsync(string userId, string code);

        //// جلب OTP صالح لمستخدم محدد (اختياري)
        //Task<OtpDto?> GetValidOtpAsync(string userId, string code);

        // Save an OTP / magic token (we accept userId or email in the first param depending on usage)
        Task SaveOtpAsync(string userIdOrEmail, string code, DateTime expires);

        // Get a stored OTP record by email/user and code (only if not used and not expired)
        Task<OtpDto?> GetValidOtpAsync(string emailOrUserId, string code);

        // Validate existence and not expired (returns bool)
        Task<bool> ValidateOtpAsync(string emailOrUserId, string code);

        // Mark OTP as used after successful verification
        Task MarkOtpUsedAsync(string emailOrUserId, string code);

        // Optional: get last token for an email (used by some implementations)
        Task<OtpDto?> GetOtpAsync(string emailOrUserId);

        // Optional: delete consumed/expired token
        Task DeleteOtpAsync(string emailOrUserId);
    }
}
