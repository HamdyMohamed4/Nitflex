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

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentRequest dto, [FromServices] PaymentFactory factory)
        {
            var gateway = factory.GetPaymentGateway(dto.CountryCode);
            var (orderId, approvalUrl, success) = await gateway.CreateOrder(dto);

            if (!success) return BadRequest("PayPal Order Creation Failed");

            return Ok(new { orderId, approvalUrl });
        }




        [HttpGet("capture")]
        public async Task<IActionResult> CaptureOrder(
    [FromQuery] string token,
    [FromServices] PaymentFactory factory,
    [FromServices] PaymentRecordService recordService)
        {
            var gateway = factory.GetPaymentGateway("EG");
            var (result, success) = await gateway.CaptureOrder(token);

            if (!success) return BadRequest(result);

            // Extract amount from PayPal capture response
            var json = JsonDocument.Parse(result);
            var amount = json.RootElement
                .GetProperty("purchase_units")[0]
                .GetProperty("payments")
                .GetProperty("captures")[0]
                .GetProperty("amount")
                .GetProperty("value")
                .GetDecimal();

            // Save in DB
            var userId = User.FindFirst("sub")?.Value ?? throw new Exception("User not found");
            await recordService.SavePaymentAsync(token, userId, amount, "Completed");

            return Ok(new { message = "Payment Captured Successfully", amount });
        }



    }
}
