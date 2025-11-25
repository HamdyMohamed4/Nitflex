using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services.Payment;
using InfrastructureLayer.Migrations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentGateway _payment;
        public PaymentController(IPaymentGateway payment)
        {
            _payment = payment;
        }
        [HttpPost("start")]
        public async Task<IActionResult> StartPayment(PaymentDto dto)
        {
            var result = await _payment.InitiatePaymentAsync(dto);
            return Ok(result);
        }

        [HttpPost("callback")]
        public async Task<IActionResult> Callback([FromBody] object payload)
        {
            bool result = await _payment.ValidatePaymentCallback(payload.ToString());
            return Ok(result);
        }



    }
}
