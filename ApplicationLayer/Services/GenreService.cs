using AutoMapper;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using Domains;
using InfrastructureLayer.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApplicationLayer.Services
{
    public class GenreService : BaseService<Genre, GenreDto>, IGenreService
    {
        private readonly IGenericRepository<Genre> _repo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public GenreService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService
        ) : base(unitOfWork, mapper, userService)
        {
            _repo = unitOfWork.Repository<Genre>();
            _mapper = mapper;
            _userService = userService;
        }

        // =========================== Custom Methods ===========================

        public async Task<List<GenreDto>> GetAllAsync()
        {
            var list = await _repo.GetAll();
            return _mapper.Map<List<GenreDto>>(list);
        }

        public async Task<GenreDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetById(id);
            return entity == null ? null : _mapper.Map<GenreDto>(entity);
        }

        public async Task<GenreDto> CreateAsync(CreateGenreDto dto)
        {
            var entity = _mapper.Map<Genre>(dto);

            entity.Id = Guid.NewGuid();
            entity.CreatedDate = DateTime.UtcNow;
            entity.CreatedBy = _userService.GetLoggedInUser();
            entity.CurrentState = 1;

            await _repo.Add(entity);

            return _mapper.Map<GenreDto>(entity);
        }

        public async Task<GenreDto?> UpdateAsync(Guid id, UpdateGenreDto dto)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return null;

            _mapper.Map(dto, entity);
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = _userService.GetLoggedInUser();

            await _repo.Update(entity);

            return _mapper.Map<GenreDto>(entity);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repo.ChangeStatus(id, _userService.GetLoggedInUser(), 0);
        }
    }
}
