using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services.Payment;
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

        [HttpGet("callback")]
        public async Task<IActionResult> HandleCallback([FromQuery] Dictionary<string, string> queryParams)
        {

            var success = await _payment.ValidatePaymentCallback(System.Text.Json.JsonSerializer.Serialize(queryParams));

            if (!success)
                return Redirect("http://localhost:4200/error");

            return Redirect("http://localhost:4200/browse");
        }



    }
}
