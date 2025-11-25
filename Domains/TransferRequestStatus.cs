using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public enum TransferRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Expired = 3
    }

    public class TransferRequest : BaseTable
    {
        public Guid ProfileId { get; set; }
        public Guid SourceUserId { get; set; }
        public Guid TargetUserId { get; set; }         // resolved at creation time
        public string TargetEmail { get; set; } = string.Empty; // store for safety
        public string Token { get; set; } = string.Empty;      // secure GUID token
        public TransferRequestStatus Status { get; set; } = TransferRequestStatus.Pending;
        public DateTime ExpiresAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
