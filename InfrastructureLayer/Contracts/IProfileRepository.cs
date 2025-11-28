using Domains;

namespace InfrastructureLayer.Contracts
{
    public interface IProfileRepository
    {
        Task<UserProfile> AddAsync(UserProfile profile);
        Task<int> CountByUserIdAsync(Guid userId);
        Task<List<UserProfile>> GetAllByUserIdAsync(Guid userId);
        Task<UserProfile?> GetByIdAsync(Guid profileId);
        Task<bool> DeleteAsync(Guid profileId, Guid userId);
        Task<bool> UpdateAsync(UserProfile profile);
    }
}
