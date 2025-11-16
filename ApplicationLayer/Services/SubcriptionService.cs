using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class SubcriptionService : BaseService<UserSubscription, UserSubscriptionDto>, ISubsciptionService
    {
        private readonly IGenericRepository<UserSubscription> _repo;
        public SubcriptionService(IGenericRepository<UserSubscription> repo, IMapper mapper, IUserService userService) : base(repo, mapper, userService)
        {
            _repo = repo;
        }

        public Task<(bool, Guid)> Add(UserSubscriptionDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeletePlanAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserSubscriptionDto?> GetCurrentUserSubscriptionAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<SubscriptionPlanDto>> GetPlansAsync()
        {
            throw new NotImplementedException();
        }

        public Task<UserSubscriptionDto> SubscribeAsync(string userId, CreateUserSubscriptionDto dto)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Update(UserSubscriptionDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanDto?> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanDto dto)
        {
            throw new NotImplementedException();
        }

        Task<List<UserSubscriptionDto>> IBaseService<UserSubscription, UserSubscriptionDto>.GetAll()
        {
            throw new NotImplementedException();
        }

        Task<UserSubscriptionDto> IBaseService<UserSubscription, UserSubscriptionDto>.GetById(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
