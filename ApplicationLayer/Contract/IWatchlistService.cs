using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IWatchlistService : IBaseService<UserWatchlist, WatchlistItemDto>
    {
        Task<List<WatchlistItemDto>> GetUserWatchlistAsync(string userId);
        Task<WatchlistItemDto> AddAsync(string userId, AddToWatchlistDto dto);
        Task<bool> RemoveAsync(string userId, Guid id);
    }
}
