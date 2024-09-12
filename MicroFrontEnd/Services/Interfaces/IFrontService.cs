using MicroFrontEnd.Models;

namespace MicroFrontEnd.Services.Interfaces
{
    public interface IFrontService
    {
        public Task PostPatientCreate(PatientViewModel patient);
        public Task PostNoteCreate(PatientNote patientNote);
        public Task<List<PatientNoteViewModel>> GetPatientManagement();
        public Task<PatientNoteViewModel> GetPatientDataAsync(int patientId);
        public Task<List<Note>> GetPatientMongoDataAsync(int patientId);
        public Task UpdatePatientData(PatientViewModel updatedPatientViewModel);
        public Task UpdateNoteData(NoteViewModel updatedNoteViewModel);
        public Task DeletePatientAndNote(int patientId);
        public Task<string> CalculateAssessmentDiabetePatient(int patientId);
    }
}