using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace ApplicationLayer.Services.Payment
{
    public class PaymobGetway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly string _apiKey;
        private readonly string _integrationId;
        private readonly string _iframeId;
        private readonly IMapper _mapper;

        public PaymobGetway(
            HttpClient httpClient,
            IConfiguration config,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper
        )
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _config = config;
            _mapper = mapper;

            _apiKey = _config["Paymob:ApiKey"];
            _integrationId = _config["Paymob:IntegrationId"];
            _iframeId = _config["Paymob:CardIframeId"];
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentDto request)
        {
            try
            {
                // 1️⃣ Auth token
                var token = await GetAuthTokenAsync();

                // 2️⃣ Register order in Paymob
                var paymobOrderId = await RegisterOrderAsync(token, request);

                // 3️⃣ Generate payment key
                var paymentKey = await GeneratePaymentKeyAsync(token, paymobOrderId, request);

                // 4️⃣ Create payment link
                string paymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_iframeId}?payment_token={paymentKey}";

                return new PaymentResponseDto
                {
                    Success = true,
                    TransactionId = paymobOrderId,
                    PaymentUrl = paymentUrl,
                    Message = "Payment initialized successfully"
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/auth/tokens",
               new { api_key = _apiKey });

            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();
            return json.RootElement.GetProperty("token").GetString();
        }

        private async Task<string> RegisterOrderAsync(string token, PaymentDto dto)
        {
            var orderRequest = new
            {
                auth_token = token,
                delivery_needed = "false",
                amount_cents = (int)(dto.Amount * 100),
                currency = "EGP",
                items = Array.Empty<object>()
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/ecommerce/orders", orderRequest);
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();

            return json.RootElement.GetProperty("id").ToString();
        }

        private async Task<string> GeneratePaymentKeyAsync(string token, string orderId, PaymentDto dto)
        {
            var billing = new
            {
                first_name = "Hamdy",
                last_name = "Mohamed",
                email = "test@gmail.com",
                phone_number = "+201111111111",
                country = "EGYPT",
                street =  "N/A",
                building = "N/A",                                   // Add building if available
                floor = "N/A",                                      // Add floor if stored
                apartment = "N/A",                                  // Add apartment if stored
                city =  "N/A",
                state = "N/A",
                postal_code = "00000"
            };

            var keyRequest = new
            {
                auth_token = token,
                amount_cents = (int)(dto.Amount * 100),
                expiration = 3600,
                order_id = orderId,
                billing_data = billing,
                currency = "EGP",
                integration_id = int.Parse(_integrationId)
            };

            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/api/acceptance/payment_keys", keyRequest);
            var json = await response.Content.ReadFromJsonAsync<JsonDocument>();

            return json.RootElement.GetProperty("token").GetString();
        }



        public async Task<bool> ValidatePaymentCallback(string payload)
        {
            var data = JsonDocument.Parse(payload).RootElement;

            bool success = data.GetProperty("success").GetBoolean();

            if (!success) return false;

            return true;
        }
    }
}
