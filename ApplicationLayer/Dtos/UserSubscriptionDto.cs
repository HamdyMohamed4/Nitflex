using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class UserSubscriptionDto:BaseDto
    {
        public Guid UserId { get; set; }
        public Guid SubscriptionPlanId { get; set; }
        public SubscriptionPlanDto? Plan { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
