using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.Payment
{
    public class PaymobGateway : IPaymentGateway
    {
        public Task<(string, bool)> CaptureOrder(string orderId)
        {
            throw new NotImplementedException();
        }

        public Task<(string, bool)> CreateOrder(CreatePaymentRequest requestData)
        {
            throw new NotImplementedException();
        }

        Task<(string orderId, string approvalUrl, bool success)> IPaymentGateway.CreateOrder(CreatePaymentRequest requestData)
        {
            throw new NotImplementedException();
        }
    }
}
