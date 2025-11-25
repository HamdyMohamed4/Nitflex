using ApplicationLayer.Contract;

using Domains;
using InfrastructureLayer;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.Services
{
    public class TransferRequestRepository : ITransferRequestRepository
    {
        private readonly NetflixContext _ctx;
        public TransferRequestRepository(NetflixContext ctx) => _ctx = ctx;

        public async Task AddAsync(TransferRequest entity)
        {
            await _ctx.Set<TransferRequest>().AddAsync(entity);
            await _ctx.SaveChangesAsync();
        }

        public async Task<TransferRequest?> GetByTokenAsync(string token)
        {
            return await _ctx.Set<TransferRequest>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        public async Task<TransferRequest?> GetByIdAsync(Guid id)
        {
            return await _ctx.Set<TransferRequest>().FindAsync(id);
        }

        public async Task UpdateAsync(TransferRequest entity)
        {
            _ctx.Set<TransferRequest>().Update(entity);
            await _ctx.SaveChangesAsync();
        }
    }
}
