using ApplicationLayer.Dtos;
using ApplicationLayer.Services;
using Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface ITicketService: IBaseService<Ticket, TicketDto>
    {
        // User
        Task<TicketDto> CreateTicketAsync(Guid userId, CreateTicketDto dto);
        Task<List<TicketDto?>> GetAllTicketsByUserAsync(Guid userId);
        Task<TicketDetailsDto?> GetTicketDetailsAsync(Guid ticketId);
        Task<TicketMessageDto> AddMessageByUserAsync(Guid ticketId, Guid senderId, TicketMessageCreateDto dto, bool isFromAdmin);
        Task<bool> DeleteTicketAsync(Guid ticketId);

        // Admin
        Task<TicketListResponseDto> GetAllTicketsAsync(int page, int pageSize);
        Task<bool> UpdateTicketStatusAsync(Guid ticketId, TicketStatus status);
    }
}
