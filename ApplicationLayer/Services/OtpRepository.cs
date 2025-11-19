using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using InfrastructureLayer;
using InfrastructureLayer.UserModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class OtpRepository : IOtpRepository
    {
        private readonly NetflixContext _db;
        private readonly IMapper _mapper;

        public OtpRepository(NetflixContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // جلب OTP صالح لمستخدم محدد
        public async Task<OtpDto?> GetValidOtpAsync(string email, string code)
        {
            var otp = await _db.EmailOtp
                .Where(x => x.Email == email && x.OtpCode == code && !x.IsUsed)
                .OrderByDescending(x => x.ExpirationDate)
                .FirstOrDefaultAsync();

            if (otp == null || otp.ExpirationDate < DateTime.UtcNow)
                return null;

            return _mapper.Map<OtpDto>(otp);
        }

        // وضع OTP كمُستخدم بعد النجاح
        public async Task MarkOtpUsedAsync(string email, string code)
        {
            var otp = await _db.EmailOtp
                .FirstOrDefaultAsync(x => x.Email == email && x.OtpCode == code && !x.IsUsed);

            if (otp != null)
            {
                otp.IsUsed = true;
                await _db.SaveChangesAsync();
            }
        }

        // تحقق من صلاحية OTP
        public async Task<bool> ValidateOtpAsync(string email, string code)
        {
            var otp = await _db.EmailOtp
                .Where(x => x.Email == email && x.OtpCode == code && !x.IsUsed)
                .OrderByDescending(x => x.ExpirationDate)
                .FirstOrDefaultAsync();

            if (otp == null) return false;

            if (otp.ExpirationDate < DateTime.UtcNow) return false;

            return true;
        }

        public async Task SaveOtpAsync(string userId, string code, DateTime expires)
        {
            var otp = new EmailOtp
            {
                Email = userId,       // لو عندك عمود Email، أو UserId حسب التصميم
                OtpCode = code,
                ExpirationDate = expires,
                CreatedDate = DateTime.UtcNow,
                IsUsed = false
            };

            await _db.EmailOtp.AddAsync(otp);
            await _db.SaveChangesAsync();
        }

        public Task<OtpDto?> GetOtpAsync(string emailOrUserId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOtpAsync(string emailOrUserId)
        {
            throw new NotImplementedException();
        }
    }
}
