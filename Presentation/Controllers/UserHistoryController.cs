using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers;
[Route("api/[controller]")]
[ApiController]
public class UserHistoryController : ControllerBase
{
    private readonly IUserHistoryService _historyService;
    private readonly IUserService _userService;

    public UserHistoryController(IUserHistoryService historyService, IUserService userService)
    {
        _historyService = historyService;
        _userService = userService;
    }

    /// <summary>
    /// Get items for the "Continue Watching" UI (ordered by last watched).
    /// </summary>
    /// <param name="limit">Maximum items to return (default 10)</param>
    [HttpGet("me/continue")]
    //[Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<List<UserHistoryDto>>>> GetContinueWatching([FromQuery] int limit = 10)
    {
        try
        {
            var userId = _userService.GetLoggedInUser();
            var items = await _historyService.GetContinueWatchingAsync(userId.ToString(), limit);
            return Ok(ApiResponse<List<UserHistoryDto>>.SuccessResponse(items, "Continue watching items retrieved"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserHistoryDto>>.FailResponse("Failed to get continue watching items", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Get full user history (paged).
    /// </summary>
    /// <param name="page">Page number (default 1)</param>
    /// <param name="pageSize">Page size (default 20)</param>
    [HttpGet("me/history")]
    //[Authorize(Roles = "User")]
    public async Task<ActionResult<ApiResponse<List<UserHistoryDto>>>> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = _userService.GetLoggedInUser();
            var history = await _historyService.GetUserHistoryAsync(userId.ToString(), page, pageSize);
            return Ok(ApiResponse<List<UserHistoryDto>>.SuccessResponse(history, "User history retrieved"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserHistoryDto>>.FailResponse("Failed to get user history", new List<string> { ex.Message }));
        }
    }
}
