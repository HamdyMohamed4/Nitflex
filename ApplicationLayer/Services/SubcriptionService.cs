using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using Presentation.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class SubcriptionService : BaseService<SubscriptionPlan, SubscriptionPlanDto>, ISubsciptionService
    {
        private readonly IGenericRepository<SubscriptionPlan> _planRepo;
        private readonly IGenericRepository<UserSubscription> _userSubRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly TokenService _tokenService;
        private readonly IRefreshTokens _refreshTokenService;


        public SubcriptionService(
            IGenericRepository<SubscriptionPlan> planRepo,
            IGenericRepository<UserSubscription> userSubRepo,
            IMapper mapper,
            IUserService userService,
            IRefreshTokens refreshTokenService,
            TokenService tokenService
        ) : base(planRepo, mapper, userService)
        {
            _planRepo = planRepo;
            _userSubRepo = userSubRepo;
            _mapper = mapper;
            _userService = userService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        // ==================== Admin: Plans ====================
        public async Task<List<SubscriptionPlanDto>> GetPlansAsync()
        {
            var plans = await _planRepo.GetAll();
            return _mapper.Map<List<SubscriptionPlanDto>>(plans);
        }

        public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid id)
        {
            var plan = await _planRepo.GetById(id);
            if (plan == null) return null;
            return _mapper.Map<SubscriptionPlanDto>(plan);
        }

        public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto)
        {
            var plan = _mapper.Map<SubscriptionPlan>(dto);
            plan.CreatedBy = _userService.GetLoggedInUser();
            plan.CurrentState = 1;

            await _planRepo.Add(plan);
            return _mapper.Map<SubscriptionPlanDto>(plan);
        }

        public async Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanDto dto)
        {
            var plan = await _planRepo.GetById(id);
            if (plan == null) return null;

            plan.Name = dto.Name;
            plan.PricePerMonth = dto.PricePerMonth;
            plan.VideoAndSoundQuality = dto.VideoAndSoundQuality;
            plan.Resolution = dto.Resolution;
            plan.SupportedDevices = dto.SupportedDevices;
            plan.MaxSimultaneousDevices = dto.MaxSimultaneousDevices;
            plan.MaxDownloadDevices = dto.MaxDownloadDevices;
            plan.SpatialAudio = dto.SpatialAudio;

            plan.UpdatedBy = _userService.GetLoggedInUser();

            await _planRepo.Update(plan);
            return _mapper.Map<SubscriptionPlanDto>(plan);
        }

        public async Task<bool> DeletePlanAsync(Guid id)
        {
            return await _planRepo.Delete(id);
        }

        // ==================== User Subscriptions ====================
        public async Task<UserSubscriptionDto?> GetCurrentUserSubscriptionAsync(string userId)
        {
            var subs = await _userSubRepo.GetList(us =>
                us.UserId.ToString() == userId &&
                us.StartDate <= DateTime.UtcNow &&
                us.EndDate >= DateTime.UtcNow);

            var currentSub = subs.FirstOrDefault();
            if (currentSub == null) return null;

            return _mapper.Map<UserSubscriptionDto>(currentSub);
        }

        public async Task<UserSubscriptionDto> SubscribeAsync(string userId, CreateUserSubscriptionDto dto)
        {
            var subPlan = await _planRepo.GetById(dto.SubscriptionPlanId);
            if (subPlan == null) throw new Exception("Invalid subscription plan");

            var userSub = new UserSubscription
            {
                Id = Guid.NewGuid(),
                Name = subPlan.Name,
                UserId = Guid.Parse(userId),
                SubscriptionPlanId = dto.SubscriptionPlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1), // أو حسب Duration
                CreatedBy = Guid.Parse(userId),
                CurrentState = 1
                
            };

            await _userSubRepo.Add(userSub);
            return _mapper.Map<UserSubscriptionDto>(userSub);
        }
        public async Task<UserSubscriptionDto> ActivateSubscriptionAsync(Guid userId, CreateUserSubscriptionDto dto)
        {
            // تفعيل الاشتراك باستخدام التفاصيل المدخلة
            var result = await SubscribeAsync(userId.ToString(), dto);

            if (result == null)
                throw new InvalidOperationException("Subscription activation failed.");

            // الحصول على بيانات المستخدم
            var user = await _userService.GetUserByIdentityAsync(userId.ToString());
            if (user == null)
                throw new InvalidOperationException("User not found.");

            // توليد التوكنات
            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // حفظ الـ Refresh Token في قاعدة البيانات
            await _refreshTokenService.Refresh(new RefreshTokenDto
            {
                Token = refreshToken,
                UserId = user.Id.ToString(),
                Expires = DateTime.UtcNow.AddDays(7),
                CurrentState = 1
            });

            // إعداد الاستجابة مع بيانات الاشتراك والتوكنات
            var response = new UserSubscriptionDto
            {
                UserId = user.Id,
                SubscriptionPlanId = dto.SubscriptionPlanId, // من التفاصيل المدخلة
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1), // على سبيل المثال
                IsActive = true, // تفعيل الاشتراك
                Name = result.Name,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return response;
        }



    }


}
