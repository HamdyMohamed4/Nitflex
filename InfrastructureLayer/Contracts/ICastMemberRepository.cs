using Domains;

namespace InfrastructureLayer.Contracts
{
    public interface ICastMemberRepository
    {
        Task<CastMember> AddAsync(CastMember entity);
        Task<CastMember?> GetByIdAsync(Guid id);
        Task<List<CastMember>> GetAllAsync();
        Task<bool> UpdateAsync(CastMember entity);
        Task<bool> DeleteAsync(Guid id);
    }
}
