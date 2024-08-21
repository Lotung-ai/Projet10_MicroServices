using MicroFrontEnd.Models;
using MicroFrontEnd.Services.Interfaces;
using MicroServices.Models;
using Microsoft.EntityFrameworkCore;

namespace MicroFrontEnd.Services
{
    public class FrontService : IFrontService
    {
        public Patient MapFrontEndToPatientApi(PatientViewModel patientViewModel)
        {
            var patient = new Patient
            {
                
                FirstName = patientViewModel.FirstName,
                LastName = patientViewModel.LastName,
                DateOfBirth = patientViewModel.DateOfBirth,
                Gender = patientViewModel.Gender,
                Address = patientViewModel.Address,
                PhoneNumber = patientViewModel.PhoneNumber
            };

            return patient;
        }
        public PatientMongo MapFrontEndToNoteApi(NoteViewModel noteViewModel)
        {
            var note= new PatientMongo
            {
                
                PatId = noteViewModel.PatId,
                Patient = noteViewModel.Patient,
                Note = noteViewModel.Note

            };

            return note;
        }


    }
}