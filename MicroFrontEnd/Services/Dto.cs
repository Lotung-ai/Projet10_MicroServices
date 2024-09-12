using MicroFrontEnd.Models;
using MicroFrontEnd.Services.Interfaces;
using MicroServiceNote.Models;
using MicroServicePatient.Models;

namespace MicroFrontEnd.Services
{
    public class Dto : IDto
    {

        public Patient MapFrontEndToPatientApi(PatientViewModel patientViewModel)
        {
            var patient = new Patient
            {
                Id = patientViewModel.Id,
                FirstName = patientViewModel.FirstName,
                LastName = patientViewModel.LastName,
                DateOfBirth = patientViewModel.DateOfBirth,
                Gender = patientViewModel.Gender,
                Address = patientViewModel.Address,
                PhoneNumber = patientViewModel.PhoneNumber
            };

            return patient;
        }

        public PatientViewModel MaPatientApiToPatientViewModel(Patient patient)
        {
            var patientViewModel = new PatientViewModel();

            patientViewModel.Id = patient.Id;
            patientViewModel.FirstName = patient.FirstName;
            patientViewModel.LastName = patient.LastName;
            patientViewModel.DateOfBirth = patient.DateOfBirth;
            patientViewModel.Gender = patient.Gender;
            patientViewModel.Address = patient.Address;
            patientViewModel.PhoneNumber = patient.PhoneNumber;

            return patientViewModel;
        }

        public Note MapFrontEndToNoteApi(NoteViewModel noteViewModel)
        {
            var note = new Note();

            note.Id = noteViewModel.Id;
            note.PatId = noteViewModel.PatId;
            note.Patient = noteViewModel.Patient;
            note.NoteText = noteViewModel.Note;

            return note;
        }

        public NoteViewModel MapNoteApiToFrontEnd(Note note)
        {
            var noteViewModel = new NoteViewModel
            {
                Id = note.Id,
                PatId = note.PatId,
                Patient = note.Patient,
                Note = note.NoteText

            };
            return noteViewModel;
        }
    }
}

