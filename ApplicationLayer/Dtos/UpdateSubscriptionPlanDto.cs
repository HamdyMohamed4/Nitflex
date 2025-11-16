using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class UpdateSubscriptionPlanDto:BaseDto
    {
        public string Name { get; set; } = null!;
        public decimal PricePerMonth { get; set; }
        public int MaxDevices { get; set; }
        public string? Resolution { get; set; }
        public string? Description { get; set; }
    }
}
