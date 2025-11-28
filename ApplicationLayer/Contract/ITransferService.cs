using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ApplicationLayer.Contract
{
    public interface ITransferService
    {
        // Called by the sender to initiate transfer and send email to receiver.
        Task<(bool Success, string Message)> InitiateTransferAsync(Guid callerUserId, InitiateProfileTransferRequest dto, string frontendConfirmationUrlBase);

        // Called by the receiver (link) to approve or reject transfer.
        Task<(bool Success, string Message)> CompleteTransferAsync(ConfirmTransferRequest dto);
    }
}
