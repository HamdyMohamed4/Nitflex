using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;

namespace ApplicationLayer.Services
{
    public class CastMemberService : ICastMemberService
    {
        private readonly ICastMemberRepository _repo;
        private readonly IMapper _mapper;

        public CastMemberService(ICastMemberRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<CastMemberDto> CreateAsync(CreateCastMemberDto dto)
        {
            var entity = new CastMember
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                PhotoUrl = dto.PhotoUrl,
                CurrentState = 1,
                CreatedDate = DateTime.UtcNow
            };

            var added = await _repo.AddAsync(entity);
            return _mapper.Map<CastMemberDto>(added);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<List<CastMemberDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return _mapper.Map<List<CastMemberDto>>(list);
        }

        public async Task<CastMemberDto?> GetByIdAsync(Guid id)
        {
            var cm = await _repo.GetByIdAsync(id);
            if (cm == null) return null;
            return _mapper.Map<CastMemberDto>(cm);
        }

        public async Task<bool> UpdateAsync(Guid id, UpdateCastMemberDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Name = dto.Name;
            existing.PhotoUrl = dto.PhotoUrl;
            existing.UpdatedDate = DateTime.UtcNow;

            return await _repo.UpdateAsync(existing);
        }
    }
}
