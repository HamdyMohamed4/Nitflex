using InfrastructureLayer.UserModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class TbPaymentTransaction : BaseTable
    {
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string PaymentProvider { get; set; }
        public string ExternalPaymentId { get; set; } // PayPal Order ID
        public decimal Amount { get; set; }
        public string Status { get; set; } // e.g., Completed, Failed, Pending
        public DateTime CreatedDate { get; set; }
    }
}
