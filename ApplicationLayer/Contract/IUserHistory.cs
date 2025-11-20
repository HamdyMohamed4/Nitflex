using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationLayer.Dtos;

namespace ApplicationLayer.Contract
{
    /// <summary>
    /// Service contract for managing and retrieving user viewing history / "Continue Watching" items.
    /// </summary>
    public interface IUserHistoryService
    {
        /// <summary>
        /// Get full history for a user (paged).
        /// </summary>
        Task<List<UserHistoryDto>> GetUserHistoryAsync(string userId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Get items appropriate for the "Continue Watching" UI.
        /// Typically ordered by last watched and limited to a small number.
        /// </summary>
        Task<List<UserHistoryDto>> GetContinueWatchingAsync(string userId, int limit = 10);

        /// <summary>
        /// Get a single history entry by its id.
        /// </summary>
        Task<UserHistoryDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// Create a new history entry or update an existing one (e.g. update position/last watched).
        /// Returns the created/updated DTO.
        /// </summary>
        Task<UserHistoryDto> AddOrUpdateAsync(UserHistoryDto dto);

        /// <summary>
        /// Remove a history entry.
        /// </summary>
        Task<bool> RemoveAsync(Guid id);

        /// <summary>
        /// Get total count of history items for a user (useful for paging).
        /// </summary>
        Task<int> GetTotalCountAsync(string userId);
    }
}

