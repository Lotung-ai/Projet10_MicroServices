using MicroServiceAuth.Models;

namespace MicroServiceAuth.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}