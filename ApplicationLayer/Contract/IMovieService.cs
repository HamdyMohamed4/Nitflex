using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IMovieService : IBaseService<Movie, MovieDto>
    {
        Task<IEnumerable<MovieDto>> GetAllAsync(Guid? genreId = null, string? search = null);
        Task<IEnumerable<MovieDto>> GetAllByFilter(MovieSearchFilterDto filter);
        Task<MovieDto?> GetByIdAsync(Guid id);
        Task<MovieDto> CreateAsync(MovieDto dto);
        Task<bool> UpdateAsync(Guid id, MovieDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
