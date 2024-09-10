using MicroServicePatient.Models;

namespace MicroServicePatient.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}