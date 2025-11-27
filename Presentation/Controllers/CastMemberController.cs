using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CastMemberController : ControllerBase
    {
        private readonly ICastMemberService _service;

        public CastMemberController(ICastMemberService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CastMemberDto>>> Create([FromBody] CreateCastMemberDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<CastMemberDto>.FailResponse("Invalid input.", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

                var created = await _service.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<CastMemberDto>.SuccessResponse(created, "Cast member created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CastMemberDto>.FailResponse("Failed to create cast member.", new List<string> { ex.Message }));
            }
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CastMemberDto>>>> GetAll()
        {
            try
            {
                var list = await _service.GetAllAsync();
                if (list == null || !list.Any())
                    return NotFound(ApiResponse<IEnumerable<CastMemberDto>>.FailResponse("No cast members found."));

                return Ok(ApiResponse<IEnumerable<CastMemberDto>>.SuccessResponse(list, "Cast members retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<IEnumerable<CastMemberDto>>.FailResponse("Failed to retrieve cast members.", new List<string> { ex.Message }));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CastMemberDto>>> GetById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(ApiResponse<CastMemberDto>.FailResponse("Invalid ID."));

                var cm = await _service.GetByIdAsync(id);
                if (cm == null)
                    return NotFound(ApiResponse<CastMemberDto>.FailResponse("Cast member not found."));

                return Ok(ApiResponse<CastMemberDto>.SuccessResponse(cm, "Cast member retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CastMemberDto>.FailResponse("Failed to retrieve cast member.", new List<string> { ex.Message }));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Update(Guid id, [FromBody] UpdateCastMemberDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<bool>.FailResponse("Invalid input.", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));

                var ok = await _service.UpdateAsync(id, dto);
                if (!ok)
                    return NotFound(ApiResponse<bool>.FailResponse("Cast member not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Cast member updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to update cast member.", new List<string> { ex.Message }));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            try
            {
                var ok = await _service.DeleteAsync(id);
                if (!ok)
                    return NotFound(ApiResponse<bool>.FailResponse("Cast member not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "Cast member deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete cast member.", new List<string> { ex.Message }));
            }
        }
    }
}
