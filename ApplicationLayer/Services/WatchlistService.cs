using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using InfrastructureLayer.UserModels;

namespace ApplicationLayer.Services
{
    public class WatchlistService : BaseService<UserWatchlist, WatchlistItemDto>, IWatchlistService
    {
        public WatchlistService(IGenericRepository<UserWatchlist> repo, IMapper mapper, IUserService userService) : base(repo, mapper) { }

        public async Task<(bool Success, Guid watchListItemId)> AddAsync(string userId, AddToWatchlistDto dto)
        {
            if (userId is null || dto is null)
                return (false, Guid.Empty);

            ApplicationUser? userFromDB = await _userService.GetUserByIdentityAsync(userId);

            if (userFromDB is null)
                return (false, Guid.Empty);

            UserWatchlist userWatchListItem = new() { ContentId = dto.ContentId, ContentType = dto.ContentType, ProfileId = dto.ProfileId, CurrentState = 1 };

            return await _repo.Add(userWatchListItem);
        }

        public async Task<List<WatchlistItemDto>> GetUserWatchlistAsync(string userId, Guid profileId)
        {
            var userFromDB = await _userService.GetUserByIdWithProfilesWithWatchListAsync(userId);

            if (userFromDB is null)
                return null!;

            UserProfile? profile = userFromDB.Profiles.FirstOrDefault(x => x.Id == profileId);

            if (profile is null)
                return null!;

            List<WatchlistItemDto> profiles = profile.WatchlistItems.Where(x => x.CurrentState == 1).Select(x => new WatchlistItemDto { Id = x.Id, ProfileId = x.ProfileId, ContentId = x.ContentId, ContentType = x.ContentType, CurrentState = x.CurrentState, AddedAt = DateTime.Now }).ToList();

            return profiles;
        }

        public async Task<bool> RemoveAsync(string userId, Guid id, Guid profileId)
        {
            var userFromDB = await _userService.GetUserByIdWithProfilesWithWatchListAsync(userId);

            if (userFromDB is null)
                return false;

            UserProfile? profile = userFromDB.Profiles.FirstOrDefault(x => x.Id == profileId);

            if (profile is null)
                return false;

            UserWatchlist? userWatchListItem = profile.WatchlistItems.FirstOrDefault(x => x.Id == id);

            if (userWatchListItem is null)
                return false;

            try
            {
                // Old Version
                // await _repo.Delete(userWatchListItem.Id); =========> Old Version By (hard deleting it total from the database)

                // soft deleting
                await ChangeStatus(userWatchListItem.Id, 0); //  =========> Updated Version by (changing status instead of deleting from the database)
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=======> namespace ApplicationLayer.Services: {ex.Message}");
            }

            return true;
        }
    }
}
