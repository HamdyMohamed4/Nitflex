using AutoMapper;
using ApplicationLayer.Contract;
using ApplicationLayer.Dtos;
using InfrastructureLayer.Contracts;
using Domains;
using Microsoft.EntityFrameworkCore;

namespace ApplicationLayer.Services
{
    public class MovieService : BaseService<Movie, MovieDto>, IMovieService
    {
        private readonly IGenericRepository<Movie> _repo;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

        public MovieService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService
        ) : base(unitOfWork, mapper, userService)
        {
            _unitOfWork = unitOfWork;
            _repo = _unitOfWork.Repository<Movie>();
            _mapper = mapper;
            _userService = userService;
        }

        // ===========================
        // Get All Movie
        // ===========================
        public async Task<IEnumerable<MovieDto>> GetAllAsync(Guid? genreId = null, string? search = null)
        {
            var query = await _repo.GetList(m => m.CurrentState == 1);

            if (genreId.HasValue)
                query = query.Where(m => m.MovieGenres.Any(g => g.GenreId == genreId.Value)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            return _mapper.Map<IEnumerable<MovieDto>>(query);
        }


        // ===========================
        // Get All with Filters
        // ===========================
        public async Task<IEnumerable<MovieDto>> GetAllByFilter(MovieSearchFilterDto filter)
        {

            var query = await _repo.GetList(m => m.CurrentState == 1);

            // ===== Search =====
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(m =>
                    m.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    m.Castings.Any(c => c.CastMember.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                    m.MovieGenres.Any(g => g.Genre.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // ===== Filter by Language =====
            if (!string.IsNullOrWhiteSpace(filter.Language))
            {
                query = query.Where(m =>
                m.Language != null && m.Language.Equals(filter.Language, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // ===== Filter by AudioType =====
            if (!string.IsNullOrWhiteSpace(filter.AudioType))
            {
                query = query.Where(m => m.AudioType.Equals(filter.AudioType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // ===== Sort =====
            if (!string.IsNullOrWhiteSpace(filter.SortOption))
            {
                query = filter.SortOption switch
                {
                    "YearRelease" => query.OrderByDescending(m => m.ReleaseYear).ToList(),
                    "A-Z" => query.OrderBy(m => m.Title).ToList(),
                    "Z-A" => query.OrderByDescending(m => m.Title).ToList(),
                    _ => query
                };
            }

            return _mapper.Map<IEnumerable<MovieDto>>(query);
        }


        // ===========================
        // Get By Id
        // ===========================
        public async Task<MovieDto?> GetByIdAsync(Guid id)
        {
            var entity = await _repo.GetById(id);
            return entity == null ? null : _mapper.Map<MovieDto>(entity);
        }

        // ===========================
        // Create
        // ===========================
        public async Task<MovieDto> CreateAsync(MovieDto dto)
        {
            var entity = _mapper.Map<Movie>(dto);

            entity.Id = Guid.NewGuid();
            entity.CreatedDate = DateTime.UtcNow;
            entity.CreatedBy = _userService.GetLoggedInUser();
            entity.CurrentState = 1;

            await _repo.Add(entity);

            return _mapper.Map<MovieDto>(entity);
        }

        // ===========================
        // Update
        // ===========================
        public async Task<bool> UpdateAsync(Guid id, MovieDto dto)
        {
            var entity = await _repo.GetById(id);
            if (entity == null) return false;

            _mapper.Map(dto, entity);

            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = _userService.GetLoggedInUser();

            return await _repo.Update(entity);
        }

        // ===========================
        // Delete (Soft Delete)
        // ===========================
        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repo.ChangeStatus(id, _userService.GetLoggedInUser(), 0);
        }
    }
}
