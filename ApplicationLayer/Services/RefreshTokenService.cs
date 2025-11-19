using AutoMapper;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.Repositories;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class RefreshTokenService : BaseService<TbRefreshTokens,RefreshTokenDto>,IRefreshTokens
    {
        IGenericRepository<TbRefreshTokens> _repo;
        IMapper _mapper;
        public RefreshTokenService(IGenericRepository<TbRefreshTokens> repo,IMapper mapper,
            IUserService userService) : base(repo,mapper, userService)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<bool> Refresh(RefreshTokenDto tokenDto)
        {
            var tokenList = await _repo.GetList(a => a.UserId == tokenDto.UserId && a.CurrentState == 1);
            foreach(var dbToken in tokenList)
            {
                _repo.ChangeStatus(dbToken.Id,Guid.Parse(tokenDto.UserId), 2);
            }

            var dbTokens = _mapper.Map<RefreshTokenDto, TbRefreshTokens>(tokenDto);
            _repo.Add(dbTokens);
            return true;
        }

        public async Task<RefreshTokenDto?> GetByTokenAsync(string token)
        {
            var entity = await _repo.GetFirstOrDefault(t => t.Token == token);

            if (entity == null) return null;

            return _mapper.Map<RefreshTokenDto>(entity);
        }
    }
}
