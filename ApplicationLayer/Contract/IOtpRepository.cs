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
        Task SaveOtpAsync(OtpDto otp);
        Task<OtpDto?> GetValidOtpAsync(string email, string code);
        Task MarkOtpUsedAsync(string email, string code);
    }
}
