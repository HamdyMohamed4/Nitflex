using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminUserController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;
        private readonly ITvShowService _tvShowService;

        public AdminUserController(IAdminUserService adminUserService, ITvShowService tvShowService)
        {
            _adminUserService = adminUserService;
            _tvShowService = tvShowService;
        }

        // ===========================
        // User CRUD
        // ===========================

        // GET: api/AdminUser
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAll()
        {
            try
            {
                var users = await _adminUserService.GetAllAsync();

                if (users == null || !users.Any())
                    return NotFound(ApiResponse<List<UserDto>>.FailResponse("No users found."));

                return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users.ToList(), "Users retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<UserDto>>.FailResponse("Failed to retrieve users.", new List<string> { ex.Message }));
            }
        }


        // GET: api/AdminUser/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id)
        {
            try
            {
                var user = await _adminUserService.GetByIdAsync(id);

                if (user == null)
                    return NotFound(ApiResponse<UserDto>.FailResponse("User not found."));

                return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDto>.FailResponse("Failed to retrieve user.", new List<string> { ex.Message }));
            }
        }


        // POST: api/AdminUser
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> Create([FromBody] CreateUserDto dto)
        {
            try
            {
                var created = await _adminUserService.CreateAsync(dto);

                return Ok(ApiResponse<UserDto>.SuccessResponse(created, "User created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<UserDto>.FailResponse("Failed to create user.", new List<string> { ex.Message }));
            }
        }


        // PUT: api/AdminUser/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(Guid id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var updated = await _adminUserService.UpdateAsync(id, dto);

                if (!updated)
                    return NotFound(ApiResponse<bool>.FailResponse("User not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(updated, "User updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to update user.", new List<string> { ex.Message }));
            }
        }


        // DELETE: api/AdminUser/{id} (Soft Delete)
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            try
            {
                var deleted = await _adminUserService.DeleteAsync(id);

                if (!deleted)
                    return NotFound(ApiResponse<bool>.FailResponse("User not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(deleted, "User soft-deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete user.", new List<string> { ex.Message }));
            }
        }


        // PATCH: api/AdminUser/{id}/block
        [HttpPatch("{id:guid}/block")]
        public async Task<ActionResult<ApiResponse<bool>>> Block(Guid id, [FromBody] UserBlockDto dto)
        {
            try
            {
                var result = await _adminUserService.BlockAsync(id, dto);

                if (!result)
                    return NotFound(ApiResponse<bool>.FailResponse("User not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, dto.Blocked ? "User blocked successfully." : "User unblocked successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to block/unblock user.", new List<string> { ex.Message }));
            }
        }

        //// GET: api/AdminUser/blocked
        //[HttpGet("blocked")]
        //public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetBlockedUsers()
        //{
        //    try
        //    {
        //        var users = await _adminUserService.GetAllUsersBlockedAsync();

        //        if (users == null || !users.Any())
        //            return NotFound(ApiResponse<List<UserDto>>.FailResponse("No blocked users found."));

        //        return Ok(ApiResponse<List<UserDto>>.SuccessResponse(users.ToList(), "Blocked users retrieved successfully."));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponse<List<UserDto>>.FailResponse("Failed to retrieve blocked users.", new List<string> { ex.Message }));
        //    }
        //}



        //// GET: api/AdminTvShow/{tvShowId}/episodes/all
        //[HttpGet("{tvShowId:guid}/episodes/all")]
        //public async Task<ActionResult<ApiResponse<List<EpisodeDto>>>> GetAllEpisodesByTvShow(Guid tvShowId)
        //{
        //    try
        //    {
        //        var episodes = await _tvShowService.GetAllEpisodesByTvShowIdAsync(tvShowId);

        //        if (episodes == null || !episodes.Any())
        //            return NotFound(ApiResponse<List<EpisodeDto>>.FailResponse("No episodes found."));

        //        return Ok(ApiResponse<List<EpisodeDto>>.SuccessResponse(episodes, "Episodes retrieved successfully."));
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ApiResponse<List<EpisodeDto>>.FailResponse("Failed to retrieve episodes.", new List<string> { ex.Message }));
        //    }
        //}
    }
}
