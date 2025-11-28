using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; }
        public string? TransactionId { get; set; }
        public string Message { get; set; }
        public string? ErrorDetails { get; set; }
    }
}
