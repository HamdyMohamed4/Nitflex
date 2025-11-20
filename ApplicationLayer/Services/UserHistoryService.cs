using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Domains;
using InfrastructureLayer.Contracts;
using AutoMapper;

namespace ApplicationLayer.Services
{
    public class UserHistoryService : IUserHistoryService
    {
        private readonly IGenericRepository<UserHistory> _repo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UserHistoryService(
            IGenericRepository<UserHistory> repo,
            IMapper mapper,
            IUserService userService)
        {
            _repo = repo;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<UserHistoryDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return null;

            // Using AutoMapper to map entity to DTO
            return _mapper.Map<UserHistoryDto>(entity);
        }

        public async Task<List<UserHistoryDto>> GetContinueWatchingAsync(string userId, int limit = 10)
        {
            if (!Guid.TryParse(userId, out var profileGuid))
                return new List<UserHistoryDto>();

            // Get list of UserHistory entities
            var items = await _repo.GetList<UserHistory>(
                filter: uh => uh.ProfileId == profileGuid,
                orderBy: uh => uh.LastWatched,
                isDescending: true);

            // Use AutoMapper to map from entity list to DTO list
            return _mapper.Map<List<UserHistoryDto>>(items.Take(limit).ToList());
        }

        public async Task<List<UserHistoryDto>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20)
        {
            if (!Guid.TryParse(userId, out var profileGuid))
                return new List<UserHistoryDto>();

            var paged = await _repo.GetPagedList<UserHistory>(
                pageNumber: page,
                pageSize: pageSize,
                filter: uh => uh.ProfileId == profileGuid,
                orderBy: uh => uh.LastWatched,
                isDescending: true);

            // Use AutoMapper to map from paged list of entities to DTOs
            return _mapper.Map<List<UserHistoryDto>>(paged.Items);
        }

        public async Task<UserHistoryDto> AddOrUpdateAsync(UserHistoryDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // determine profile id
            var profileId = dto.ProfileId;

            // Try to find existing record for the same profile + content
            var existing = await _repo.GetFirstOrDefault(uh =>
                uh.ProfileId == profileId &&
                uh.ContentId == dto.ContentId &&
                uh.ContentType == dto.ContentType);

            if (existing != null)
            {
                // Update fields
                existing.Position = dto.Position;
                existing.Duration = dto.Duration;
                existing.LastWatched = dto.LastWatchedAt == default ? DateTime.UtcNow : dto.LastWatchedAt;
                existing.UpdatedBy = _userService.GetLoggedInUser();

                await _repo.Update(existing);
                // Return the updated entity as DTO
                return _mapper.Map<UserHistoryDto>(existing);
            }
            else
            {
                // Create new entity if not found
                var entity = new UserHistory
                {
                    Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                    ProfileId = profileId,
                    ContentId = dto.ContentId,
                    ContentType = dto.ContentType,
                    Position = dto.Position,
                    Duration = dto.Duration,
                    LastWatched = dto.LastWatchedAt == default ? DateTime.UtcNow : dto.LastWatchedAt,
                    CurrentState = dto.CurrentState > 0 ? dto.CurrentState : 1,
                    CreatedBy = _userService.GetLoggedInUser()
                };

                await _repo.Add(entity);

                // Return the created entity as DTO
                return _mapper.Map<UserHistoryDto>(entity);
            }
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            return await _repo.Delete(id);
        }

        public async Task<int> GetTotalCountAsync(string userId)
        {
            if (!Guid.TryParse(userId, out var profileGuid))
                return 0;

            var list = await _repo.GetList(uh => uh.ProfileId == profileGuid);
            return list.Count;
        }
    }
}
