using InfrastructureLayer.UserModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domains
{
    public class UserSubscription : BaseTable
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string PaymentStatus { get; set; } = "Paid";

        // Foreign Keys to ApplicationUser and SubscriptionPlan (Changed from int/custom User to Guid/ApplicationUser)
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = default!; // Renamed reference for clarity

        public Guid SubscriptionPlanId { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; } = default!;
    }

}
