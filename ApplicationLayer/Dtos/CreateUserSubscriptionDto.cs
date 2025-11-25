using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class CreateUserSubscriptionDto
    {
        public Guid SubscriptionPlanId { get; set; } 
        public string Token { get; set; }


    }
}
