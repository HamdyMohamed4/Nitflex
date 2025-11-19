using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchListController : ControllerBase
{
    private readonly IWatchlistService _watchListService;

    public WatchListController(IWatchlistService watchListService)
    {
        _watchListService = watchListService;
    }

    [HttpPost]
    [Route("AddToWatchList")]
    public async Task<ActionResult<WatchlistItemDto>> AddAsync(string userId, AddToWatchlistDto dto)
    {
        var result = await _watchListService.AddAsync(userId, dto);

        if (!result.Success)
            return BadRequest();

        return Ok(result);
    }

    [HttpPost]
    [Route("GetUserWatchlistAsync")]
    public async Task<ActionResult<List<WatchlistItemDto>>> GetUserWatchlistAsync(string userId, Guid profileId)
    {
        var result = await _watchListService.GetUserWatchlistAsync(userId, profileId);

        return result is null ? BadRequest() : Ok(result);
    }

    [HttpPost]
    [Route("RemoveAsync")]
    public async Task<ActionResult<bool>> RemoveAsync(string userId, Guid id, Guid profileId)
    {
        var result = await _watchListService.RemoveAsync(userId, id, profileId);

        return !result ? BadRequest() : Ok(new { Success = true, Message = "Deleted Successfully" });
    }
}