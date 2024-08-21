using MicroFrontEnd.Models;
using MicroServices.Models;

namespace MicroFrontEnd.Services.Interfaces
{
    public interface IFrontService
    {
        public Patient MapFrontEndToPatientApi(PatientViewModel patientViewModel);
        public PatientMongo MapFrontEndToNoteApi(NoteViewModel noteViewModel);

    }
}