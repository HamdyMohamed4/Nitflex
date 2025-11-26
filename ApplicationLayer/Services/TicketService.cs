using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using AutoMapper;
using Domains;
using InfrastructureLayer.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class TicketService : BaseService<Ticket, TicketDto>, ITicketService
    {
        private readonly IGenericRepository<Ticket> _ticketRepo;
        private readonly IGenericRepository<TicketMessage> _messageRepo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdminUserService _adminUserService;

        public TicketService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService,
            IGenericRepository<Ticket> ticketRepo,
            IGenericRepository<TicketMessage> messageRepo,
            IAdminUserService adminUserService
  

        ) : base(unitOfWork, mapper, userService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userService = userService;
            _ticketRepo = ticketRepo;
            _messageRepo = messageRepo;
            _adminUserService = adminUserService;
   
        }

        // ==========================
        // User Methods
        // ==========================

        //Create Ticket By User
        public async Task<TicketDto> CreateTicketAsync(Guid userId, CreateTicketDto dto)
        {
            var user = await _adminUserService.GetByIdAsync(userId);

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                UserId = userId,
                UserName = user?.FullName,
                CurrentState = 1,
                Status = TicketStatus.New,
                CreatedDate = DateTime.UtcNow
            };

            await _ticketRepo.Add(ticket);
            await _unitOfWork.SaveChangesAsync();

            var message = new TicketMessage
            {
                Id = Guid.NewGuid(),
                TicketId = ticket.Id,
                SenderId = userId,
                Message = dto.MessageContent, 
                IsFromAdmin = false, //the first message from user
                SentDate = DateTime.UtcNow
            };

            await _messageRepo.Add(message);
            await _unitOfWork.SaveChangesAsync();

            var ticketDto = _mapper.Map<TicketDto>(ticket);

            return ticketDto;
        }

        //Get All Tickets For User
        public async Task<List<TicketDto>> GetAllTicketsByUserAsync(Guid userId)
        {
            var tickets = await _ticketRepo.GetList(t => t.UserId == userId && t.CurrentState == 1);
            return _mapper.Map<List<TicketDto>>(tickets);
        }

        //Get Ticket Detail
        public async Task<TicketDetailsDto> GetTicketDetailsAsync(Guid ticketId)
        {
            var ticket = await _ticketRepo.GetFirstOrDefault(t => t.Id == ticketId && t.CurrentState == 1);

            if (ticket == null) return null;

            ticket.Messages = await _messageRepo.GetList(m => m.TicketId == ticketId);

            return _mapper.Map<TicketDetailsDto>(ticket);

        }

        //Add Message By User or Admin
        public async Task<TicketMessageDto> AddMessageByUserAsync(Guid ticketId, Guid senderId, TicketMessageCreateDto dto, bool isFromAdmin)
        {
            var ticket = await _ticketRepo.GetById(ticketId);

            if (ticket == null || ticket.CurrentState == 0) return null;

            var message = new TicketMessage
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                SenderId = senderId,
                Message = dto.Message,
                IsFromAdmin = isFromAdmin,
                SentDate = DateTime.UtcNow
            };

            await _messageRepo.Add(message);
            await _unitOfWork.SaveChangesAsync();

            var msgDto = _mapper.Map<TicketMessageDto>(message);

            if (!isFromAdmin) msgDto.SenderName = ticket.UserName;
            else msgDto.SenderName = "Admin";

            return msgDto;
        }

        //Delete Ticket By User
        public async Task<bool> DeleteTicketAsync(Guid ticketId)
        {
            var ticket = await _ticketRepo.GetById(ticketId);

            if (ticket == null || ticket.CurrentState == 0) return false;

            ticket.CurrentState = 0;
            ticket.UpdatedDate = DateTime.UtcNow;

            var result = await _ticketRepo.Update(ticket);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }

        // ==========================
        // Admin Methods
        // ==========================

        //Get All Tickets For All User
        public async Task<TicketListResponseDto> GetAllTicketsAsync(int page, int pageSize)
        {
            var tickets = await _ticketRepo.GetList(
                t => t.CurrentState == 1,
                t => t.CreatedDate, // order by
                isDescending: true
            );

            var pagedTickets = tickets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new TicketListResponseDto
            {
                Tickets = _mapper.Map<List<TicketDto>>(pagedTickets),
                TotalCount = tickets.Count,
                Page = page,
                PageSize = pageSize
            };
        }

        //Update Ticket Status For User
        public async Task<bool> UpdateTicketStatusAsync(Guid ticketId, TicketStatus status)
        {
            var ticket = await _ticketRepo.GetById(ticketId);

            if (ticket == null) return false;

            ticket.Status = status;
            ticket.UpdatedDate = DateTime.UtcNow;

            var result = await _ticketRepo.Update(ticket);

            if (result)
                await _unitOfWork.SaveChangesAsync();

            return result;
        }
    }
}
