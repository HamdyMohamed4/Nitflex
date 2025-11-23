using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IPaymentGateway
    {
        //public Task<(string, bool)> CreateOrder(CreatePaymentRequest requestData);

        public Task<(string, bool)> CaptureOrder(string orderId);
        Task<(string orderId, string approvalUrl, bool success)> CreateOrder(CreatePaymentRequest requestData);
    }
}
