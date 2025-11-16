using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class CreateUserSubscriptionDto:BaseDto
    {
        public Guid SubscriptionPlanId { get; set; }
        public int Months { get; set; } = 1;
    }
}
