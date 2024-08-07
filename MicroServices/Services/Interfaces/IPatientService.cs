using MicroServices.Models;

namespace MicroServices.Services.Interfaces
{
    public interface IPatientService
    {       
            Task<Patient> CreatePatientAsync(Patient Patient);
            Task<Patient> GetPatientByIdAsync(int id);
            Task<IEnumerable<Patient>> GetAllPatientsAsync();
            Task<Patient> UpdatePatientAsync(Patient Patient);
            Task<bool> DeletePatientAsync(int id);
        
    }
}