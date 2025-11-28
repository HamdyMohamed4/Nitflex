using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domains;

namespace ApplicationLayer.Contract
{
    public interface ITransferRequestRepository
    {
        Task AddAsync(TransferRequest entity);
        Task<TransferRequest?> GetByTokenAsync(string token);
        Task<TransferRequest?> GetByIdAsync(Guid id);
        Task UpdateAsync(TransferRequest entity);
    }
}
