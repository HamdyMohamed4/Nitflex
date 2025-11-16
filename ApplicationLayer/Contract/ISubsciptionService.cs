using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface ISubsciptionService:IBaseService<UserSubscription, UserSubscriptionDto>
    {
        // Plans - Admin
        Task<List<SubscriptionPlanDto>> GetPlansAsync();
        Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid id);
        Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto);
        Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanDto dto);
        Task<bool> DeletePlanAsync(Guid id);

        // User subscriptions
        Task<UserSubscriptionDto?> GetCurrentUserSubscriptionAsync(string userId);
        Task<UserSubscriptionDto> SubscribeAsync(string userId, CreateUserSubscriptionDto dto);

    }
}
