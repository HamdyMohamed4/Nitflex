using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IRatingService : IBaseService<UserRating, RatingDto>
    {
        Task<RatingDto> RateAsync(string userId, RateContentDto dto);
        Task<double?> GetAverageRatingAsync(string contentType, Guid contentId);
    }
}
