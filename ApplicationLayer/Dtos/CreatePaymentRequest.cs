using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class CreatePaymentRequest
    {
        public List<CartItemDto> Items { get; set; }
        public decimal ShippingValue { get; set; }
    }
}
