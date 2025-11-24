using ApplicationLayer.Contract;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services.Payment
{
    public class PaymentFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public PaymentFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentGateway GetPaymentGateway(string countryCode)
        {
            if (countryCode == "EG")
                return _serviceProvider.GetRequiredService<PayPalGateway>();
            else
                return _serviceProvider.GetRequiredService<PayPalGateway>();
        }
    }
}
