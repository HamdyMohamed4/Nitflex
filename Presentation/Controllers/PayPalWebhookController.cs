using InfrastructureLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalWebhookController : ControllerBase
    {

        private readonly NetflixContext _db;

        public PayPalWebhookController(NetflixContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Listener([FromBody] JsonElement webhookEvent)
        {
            var eventType = webhookEvent.GetProperty("event_type").GetString();

            if (eventType == "PAYMENT.CAPTURE.COMPLETED")
            {
                var orderId = webhookEvent.GetProperty("resource").GetProperty("id").GetString();

                var payment = await _db.TbPaymentTransaction.FirstOrDefaultAsync(x => x.ExternalPaymentId == orderId);
                if (payment != null)
                {
                    payment.Status = "Completed";
                    await _db.SaveChangesAsync();
                }
            }

            return Ok();
        }
    }
}
