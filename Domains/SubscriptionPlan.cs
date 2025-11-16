using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class SubscriptionPlan : BaseTable
    {
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PricePerMonth { get; set; }
        public int MaxDevices { get; set; }
        [MaxLength(50)]
        public string Resolution { get; set; } = "HD";
        public string Description { get; set; } = string.Empty;

        public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    }

}
