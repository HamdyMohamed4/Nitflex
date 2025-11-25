using System;

namespace ApplicationLayer.Dtos
{
    public class TransferProfileRequest
    {
        /// <summary>
        /// Target user id that will receive the profile.
        /// </summary>
        public Guid TargetUserId { get; set; }
    }
}
