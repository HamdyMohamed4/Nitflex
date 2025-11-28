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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ApplicationLayer.Services
{
    public class RatingService : BaseService<UserRating,RatingDto>, IRatingService
    {
        private readonly IGenericRepository<UserRating> _repo;
        public RatingService(IGenericRepository<UserRating> repo, IMapper mapper, IUserService userService) : base(repo, mapper)
        {
            _repo = repo;
        }

        public Task<double?> GetAverageRatingAsync(string contentType, Guid contentId)
        {
            throw new NotImplementedException();
        }

        public Task<RatingDto> RateAsync(string userId, RateContentDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
