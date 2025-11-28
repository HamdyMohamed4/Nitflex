using ApplicationLayer.Dtos;
using Domains;

namespace ApplicationLayer.Contract
{
    public interface ICastMemberService
    {
        Task<CastMemberDto> CreateAsync(CreateCastMemberDto dto);
        Task<List<CastMemberDto>> GetAllAsync();
        Task<CastMemberDto?> GetByIdAsync(Guid id);
        Task<bool> UpdateAsync(Guid id, UpdateCastMemberDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
