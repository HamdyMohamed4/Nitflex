using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class CreatePaymentRequest
    {
        public string CountryCode { get; set; } = "EG";
        public string SuccessUrl { get; set; }
        public string CancelUrl { get; set; }
        public List<CartItemDto> Items { get; set; }

    }
}
