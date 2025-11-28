using Domains;
using InfrastructureLayer.Contracts;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureLayer.Repositories
{
    public class CastMemberRepository : ICastMemberRepository
    {
        private readonly NetflixContext _ctx;

        public CastMemberRepository(NetflixContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<CastMember> AddAsync(CastMember entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await _ctx.CastMembers.AddAsync(entity);
            await _ctx.SaveChangesAsync();
            _ctx.Entry(entity).State = EntityState.Detached;
            return entity;
        }

        public async Task<List<CastMember>> GetAllAsync()
        {
            return await _ctx.CastMembers.AsNoTracking().ToListAsync();
        }

        public async Task<CastMember?> GetByIdAsync(Guid id)
        {
            return await _ctx.CastMembers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> UpdateAsync(CastMember entity)
        {
            var db = await _ctx.CastMembers.FirstOrDefaultAsync(c => c.Id == entity.Id);
            if (db == null) return false;

            db.Name = entity.Name;
            db.PhotoUrl = entity.PhotoUrl;
            db.Bio = entity.Bio;
            db.UpdatedDate = DateTime.UtcNow;

            _ctx.Entry(db).State = EntityState.Modified;
            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var db = await _ctx.CastMembers.FirstOrDefaultAsync(c => c.Id == id);
            if (db == null) return false;
            _ctx.CastMembers.Remove(db);
            await _ctx.SaveChangesAsync();
            return true;
        }
    }
}
