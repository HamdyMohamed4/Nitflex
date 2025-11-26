using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.UserModels;
using Microsoft.AspNetCore.Http;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PaymobGetway(
            HttpClient httpClient,
            IConfiguration config,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _httpClient = httpClient;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _config = config;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;

            _apiKey = _config["Paymob:ApiKey"];
            _integrationId = _config["Paymob:IntegrationId"];
            _iframeId = _config["Paymob:CardIframeId"];
        }

        public async Task<PaymentResponseDto> InitiatePaymentAsync(PaymentDto request)
        {
            try
            {
                var token = await GetAuthTokenAsync();
                var paymobOrderId = await RegisterOrderAsync(token, request);
                var paymentKey = await GeneratePaymentKeyAsync(token, paymobOrderId, request);

                // Save pending transaction
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                await _unitOfWork.Repository<TbPaymentTransaction>().AddAsync(new TbPaymentTransaction
                {
                    UserId = Guid.Parse(userId),
                    PaymentProvider = "Paymob",
                    ExternalPaymentId = paymobOrderId,
                    Amount = request.Amount,
                    Status = "Pending",
                    CreatedDate = DateTime.UtcNow
                });

                await _unitOfWork.SaveChangesAsync();

                return new PaymentResponseDto
                {
                    Success = true,
                    TransactionId = paymobOrderId,
                    PaymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_iframeId}?payment_token={paymentKey}",
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
            var response = await _httpClient.PostAsJsonAsync(
                "https://accept.paymob.com/api/auth/tokens",
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
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);

            var names = (user.FullName ?? "Unknown User").Split(' ', 2);
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[1] : "N/A";

            var billing = new
            {
                first_name = firstName,
                last_name = lastName,
                email = user.Email ?? "unknown@mail.com",
                phone_number = user.PhoneNumber ?? "+201000000000",
                country = "EGYPT",
                street = "N/A",
                building = "N/A",
                floor = "N/A",
                apartment = "N/A",
                city = "N/A",
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
            try
            {
                var json = JsonDocument.Parse(payload);
                var root = json.RootElement;

                string externalPaymentId = null;

                // Detect if "order" is a string or an object
                if (root.GetProperty("order").ValueKind == JsonValueKind.Object)
                {
                    externalPaymentId = root.GetProperty("order").GetProperty("id").GetString();
                }
                else
                {
                    externalPaymentId = root.GetProperty("order").GetString();
                }

                bool success = root.TryGetProperty("success", out var successElement)
                               && (successElement.ValueKind == JsonValueKind.String
                                   ? bool.Parse(successElement.GetString())
                                   : successElement.GetBoolean());

                var transaction = await _unitOfWork.Repository<TbPaymentTransaction>()
                    .GetFirstOrDefault(x => x.ExternalPaymentId == externalPaymentId);


                if (transaction == null)
                    return false;

                transaction.Status = success ? "Completed" : "Failed";
               await _unitOfWork.Repository<TbPaymentTransaction>().Update(transaction);


                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing callback: {ex.Message}");
                return false;
            }
        }

    }
}
