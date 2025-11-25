using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TransferProfileRequest
    {
        /// <summary>
        /// Target account (existing user) that will receive the profile data.
        /// </summary>
        public Guid TargetUserId { get; set; }
    }
}
