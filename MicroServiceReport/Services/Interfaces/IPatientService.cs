using MicroServiceReport.Models;

namespace MicroServiceReport.Services.Interfaces
{
    public interface IPatientService
    {
        Task<Patient> GetPatientByIdAsync(int id);
        Task<Patient> GetPatientByNameDob(string firstName, string lastName, DateTime dateOfBirth);
        Task<List<Patient>> GetAllPatientsAsync();
    }
}