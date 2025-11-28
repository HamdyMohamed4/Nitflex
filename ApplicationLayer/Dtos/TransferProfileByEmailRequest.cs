using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Dtos
{
    public class TransferProfileByEmailRequest
    {
        /// <summary>
        /// Profile to be transferred.
        /// </summary>
        public Guid ProfileId { get; set; }

        /// <summary>
        /// Target account email that will receive this profile.
        /// </summary>
        public string Email { get; set; } = string.Empty;
    }
}
