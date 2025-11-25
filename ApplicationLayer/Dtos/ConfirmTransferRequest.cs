using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class ConfirmTransferRequest
    {
        public string Token { get; set; } = string.Empty;
        public bool Approve { get; set; }
    }
}
