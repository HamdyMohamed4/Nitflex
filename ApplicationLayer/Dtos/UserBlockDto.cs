using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class UserBlockDto
    {
        public bool Blocked { get; set; } // true = block, false = unblock
        public string? Reason { get; set; } 
    }

}
