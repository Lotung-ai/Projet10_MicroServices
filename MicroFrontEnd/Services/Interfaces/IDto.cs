using MicroFrontEnd.Models;
using MicroServiceNote.Models;
using MicroServicePatient.Models;

namespace MicroFrontEnd.Services.Interfaces
{
    public interface IDto
    {
        public Patient MapFrontEndToPatientApi(PatientViewModel patientViewModel);
        public Note MapFrontEndToNoteApi(NoteViewModel noteViewModel);
        public PatientViewModel MaPatientApiToPatientViewModel(Patient patient);
        public NoteViewModel MapNoteApiToFrontEnd(Note note);
    }
}