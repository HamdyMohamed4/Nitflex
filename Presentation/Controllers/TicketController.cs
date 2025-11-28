using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // ===========================
        // User actions
        // ===========================

        // POST: api/Ticket
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<TicketDto>>> CreateTicket([FromQuery] Guid userId, [FromBody] CreateTicketDto dto)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(ApiResponse<TicketDto>.FailResponse("Invalid user id."));

                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<TicketDto>.FailResponse("Invalid data."));

                var ticket = await _ticketService.CreateTicketAsync(userId, dto);

                return Ok(ApiResponse<TicketDto>.SuccessResponse(ticket, "Ticket created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketDto>.FailResponse("Failed to create ticket", new List<string> { ex.Message }));
            }
        }

        // GET: api/Ticket/user/{userId}
        [HttpGet("user/{userId:guid}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<List<TicketDto>>>> GetTicketsByUser(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(ApiResponse<List<TicketDto>>.FailResponse("Invalid user id."));

                var tickets = await _ticketService.GetAllTicketsByUserAsync(userId);

                if (tickets == null || tickets.Count == 0)
                    return NotFound(ApiResponse<List<TicketDto>>.FailResponse("No tickets found for this user."));

                return Ok(ApiResponse<List<TicketDto>>.SuccessResponse(tickets, "Tickets retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<List<TicketDto>>.FailResponse("Failed to retrieve tickets", new List<string> { ex.Message }));
            }
        }

        // GET: api/Ticket/{ticketId}
        [HttpGet("{ticketId:guid}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<TicketDetailsDto>>> GetTicketDetails(Guid ticketId)
        {
            try
            {
                if (ticketId == Guid.Empty)
                    return BadRequest(ApiResponse<TicketDetailsDto>.FailResponse("Invalid ticket id."));

                var ticket = await _ticketService.GetTicketDetailsAsync(ticketId);

                if (ticket == null)
                    return NotFound(ApiResponse<TicketDetailsDto>.FailResponse("Ticket not found."));

                return Ok(ApiResponse<TicketDetailsDto>.SuccessResponse(ticket, "Ticket details retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketDetailsDto>.FailResponse("Failed to retrieve ticket details", new List<string> { ex.Message }));
            }
        }

        // POST: api/Ticket/{ticketId}/messages
        [HttpPost("{ticketId:guid}/messages")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<TicketMessageDto>>> AddMessage(Guid ticketId, [FromQuery] Guid senderId, [FromBody] TicketMessageCreateDto dto, [FromQuery] bool isFromAdmin = false)
        {
            try
            {
                if (ticketId == Guid.Empty || senderId == Guid.Empty)
                    return BadRequest(ApiResponse<TicketMessageDto>.FailResponse("Invalid ticket id or sender id."));

                if (!ModelState.IsValid)
                    return BadRequest(ApiResponse<TicketMessageDto>.FailResponse("Invalid data."));

                var message = await _ticketService.AddMessageByUserAsync(ticketId, senderId, dto, isFromAdmin);

                if (message == null)
                    return NotFound(ApiResponse<TicketMessageDto>.FailResponse("Ticket not found or deleted."));

                return Ok(ApiResponse<TicketMessageDto>.SuccessResponse(message, "Message added successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketMessageDto>.FailResponse("Failed to add message", new List<string> { ex.Message }));
            }
        }

        // DELETE: api/Ticket/{ticketId}
        [HttpDelete("{ticketId:guid}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteTicket(Guid ticketId)
        {
            try
            {
                var result = await _ticketService.DeleteTicketAsync(ticketId);

                if (!result)
                    return NotFound(ApiResponse<bool>.FailResponse("Ticket not found or already deleted."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Ticket deleted successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to delete ticket.", new List<string> { ex.Message }));
            }
        }


        // ===========================
        // Admin actions
        // ===========================

        // GET: api/Ticket?page=1&pageSize=20
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<TicketListResponseDto>>> GetAllTickets([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var tickets = await _ticketService.GetAllTicketsAsync(page, pageSize);

                return Ok(ApiResponse<TicketListResponseDto>.SuccessResponse(tickets, "All tickets retrieved successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<TicketListResponseDto>.FailResponse("Failed to retrieve tickets", new List<string> { ex.Message }));
            }
        }

        // PUT: api/Ticket/{ticketId}/status
        [HttpPut("{ticketId:guid}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateTicketStatus(Guid ticketId, [FromQuery] TicketStatus status)
        {
            try
            {
                var result = await _ticketService.UpdateTicketStatusAsync(ticketId, status);

                if (!result)
                    return NotFound(ApiResponse<bool>.FailResponse("Ticket not found."));

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Ticket status updated successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<bool>.FailResponse("Failed to update ticket status.", new List<string> { ex.Message }));
            }
        }
    }
}
