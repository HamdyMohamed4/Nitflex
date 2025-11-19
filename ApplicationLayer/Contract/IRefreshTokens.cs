using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IRefreshTokens : IBaseService<TbRefreshTokens, RefreshTokenDto>
    {
        Task<bool> Refresh(RefreshTokenDto tokenDto);

        Task<RefreshTokenDto?> GetByTokenAsync(string token);

    }
}
