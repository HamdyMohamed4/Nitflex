using ApplicationLayer.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.Contract
{
    public interface IAdminUserService
    {
        Task<IEnumerable<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(Guid userId);
        Task<UserDto> CreateAsync(CreateUserDto dto);
        Task<bool> UpdateAsync(Guid userId, UpdateUserDto dto);
        Task<bool> DeleteAsync(Guid userId);
        Task<bool> BlockAsync(Guid userId, UserBlockDto dto);
        Task<IEnumerable<UserDto>> GetAllUsersBlockedAsync();
    }

}
