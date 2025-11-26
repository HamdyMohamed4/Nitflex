using Domains;
using InfrastructureLayer.Contracts;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly NetflixContext _ctx;

        public ProfileRepository(NetflixContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<UserProfile> AddAsync(UserProfile profile)
        {
            profile.CreatedDate = DateTime.UtcNow;
            await _ctx.Profiles.AddAsync(profile);
            await _ctx.SaveChangesAsync();
            // detach to avoid tracking surprises
            _ctx.Entry(profile).State = EntityState.Detached;
            return profile;
        }

        public async Task<int> CountByUserIdAsync(Guid userId)
        {
            return await _ctx.Profiles.CountAsync(p => p.UserId == userId);
        }

        public async Task<List<UserProfile>> GetAllByUserIdAsync(Guid userId)
        {
            return await _ctx.Profiles
                .Where(p => p.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserProfile?> GetByIdAsync(Guid profileId)
        {
            return await _ctx.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == profileId);
        }

        public async Task<bool> DeleteAsync(Guid profileId, Guid userId)
        {
            var profile = await _ctx.Profiles.FirstOrDefaultAsync(p => p.Id == profileId && p.UserId == userId);
            if (profile == null) return false;
            _ctx.Profiles.Remove(profile);
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
