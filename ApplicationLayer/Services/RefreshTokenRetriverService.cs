using AutoMapper;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.Contracts;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class RefreshTokenRetriverService : IRefreshTokenRetriver
    {
        IGenericRepository<TbRefreshTokens> _repo;
        IMapper _mapper;
        public RefreshTokenRetriverService(IGenericRepository<TbRefreshTokens> repo, IMapper mapper) 
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<RefreshTokenDto> GetByToken(string token)
        {
            var refreshToken = await _repo.GetFirstOrDefault(a => a.Token == token);
            return _mapper.Map<TbRefreshTokens, RefreshTokenDto>(refreshToken);
        }
    }
}
