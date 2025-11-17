using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using InfrastructureLayer;
using Microsoft.EntityFrameworkCore;


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

        public async Task SaveOtpAsync(OtpDto otp)
        {
            var entity = _mapper.Map<EmailOtp>(otp);
            await _db.EmailOtp.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        //public async Task<OtpDto?> GetValidOtpAsync(string email, string code)
        //{
        //    var otp = await _db.EmailOtp
        //        .Where(x => x.Email == email && x.OtpCode == code && !x.IsUsed)
        //        .OrderByDescending(x => x.ExpirationDate)
        //        .FirstOrDefaultAsync();

        //    return otp == null ? null : _mapper.Map<OtpDto>(otp);
        //}

        public async Task<OtpDto?> GetValidOtpAsync(string email, string code)
        {
            var otp = await _db.EmailOtp
                .Where(x => x.Email == email && x.OtpCode == code && !x.IsUsed)
                .OrderByDescending(x => x.ExpirationDate)
                .FirstOrDefaultAsync();

            if (otp == null)
            {
                Console.WriteLine("❌ OTP Not Found OR Already Used.");
                return null;
            }

            if (otp.ExpirationDate < DateTime.UtcNow)
            {
                Console.WriteLine("⏳ OTP Expired");
                return null;
            }

            Console.WriteLine($"✅ OTP Found: {otp.OtpCode}, Email: {otp.Email}, Exp: {otp.ExpirationDate}");

            return _mapper.Map<OtpDto>(otp);
        }





        public async Task MarkOtpUsedAsync(string email, string code)
        {
            var otp = await _db.EmailOtp
                .FirstOrDefaultAsync(x => x.Email == email && x.OtpCode == code);

            if (otp != null)
            {
                otp.IsUsed = true;
                await _db.SaveChangesAsync();
            }
        }

    }

}
