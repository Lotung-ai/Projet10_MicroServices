using MicroFrontEnd.Models;
using MicroServices.Models;

namespace MicroFrontEnd.Services.Interfaces
{
    public interface IFrontService
    {
        public Patient MapFrontEndToPatientApi(PatientViewModel patientViewModel);
        public Note MapFrontEndToNoteApi(NoteViewModel noteViewModel);
        public PatientViewModel MaPatientApiToPatientViewModel(Patient patient);
        public NoteViewModel MapNoteApiToFrontEnd(Note note);
        public Task PostPatientCreate(PatientViewModel patient);
        public Task PostNoteCreate(PatientNote patientNote);
        public Task<PatientNoteViewModel> GetPatientDataAsync(int patientId);
        public Task<List<Note>> GetPatientMongoDataAsync(int patientId);
        public Task UpdatePatientData(PatientViewModel updatedPatientViewModel);
        public Task UpdateNoteData(NoteViewModel updatedNoteViewModel);
    }
}