using ApplicationLayer.Dtos;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IGenreService : IBaseService<Genre,GenreDto>
    {
        Task<List<GenreDto>> GetAllAsync();
        Task<GenreDto?> GetByIdAsync(Guid id);
        Task<GenreDto> CreateAsync(CreateGenreDto dto);
        Task<GenreDto?> UpdateAsync(Guid id, UpdateGenreDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
