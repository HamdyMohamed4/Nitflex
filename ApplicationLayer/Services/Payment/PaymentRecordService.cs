using Domains;
using InfrastructureLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.Payment
{
    public class PaymentRecordService
    {
        private readonly NetflixContext _db;

        public PaymentRecordService(NetflixContext db)
        {
            _db = db;
        }

        public async Task<TbPaymentTransaction> SavePaymentAsync(string orderId, string userId, decimal amount, string status)
        {
            var transaction = new TbPaymentTransaction
            {
                UserId = Guid.Parse(userId),
                Amount = amount,
                PaymentProvider = "PayPal",
                ExternalPaymentId = orderId,
                Status = status,
                CreatedDate = DateTime.UtcNow
            };

            await _db.TbPaymentTransaction.AddAsync(transaction);
            await _db.SaveChangesAsync();

            return transaction;
        }
    }
}
