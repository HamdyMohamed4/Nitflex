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
        // حفظ OTP جديد
        Task SaveOtpAsync(string userId, string code, DateTime expires);

        // التحقق من صلاحية OTP (صالح ولم يُستخدم)
        Task<bool> ValidateOtpAsync(string userId, string code);

        // وضع OTP كمُستخدم (used)
        Task MarkOtpUsedAsync(string userId, string code);

        // جلب OTP صالح لمستخدم محدد (اختياري)
        Task<OtpDto?> GetValidOtpAsync(string userId, string code);
    }
}
