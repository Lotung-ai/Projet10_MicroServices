using MicroServices.Models;

namespace MicroServices.Services.Interfaces
{
    public interface INoteService
    {
        public Task<Note?> GetAsync(string id);
        public Task<List<Note>> GetPatientByPatIdAsync(int patId);
    }
}