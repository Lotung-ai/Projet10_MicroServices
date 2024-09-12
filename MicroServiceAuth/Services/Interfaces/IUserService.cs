using MicroServiceAuth.Models;

namespace MicroServiceAuth.Services.Interfaces
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<User> GetUserByIdAsync(int id);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
    }
}