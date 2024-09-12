using MicroServiceReport.Models;
using MicroServiceReport.Services.Interfaces;
using System.Text.RegularExpressions;

namespace MicroServiceReport.Services
{
    public class ReportService : IReportService
    {

        private readonly IPatientService _patientService;
        private readonly INoteService _noteService;

        private readonly List<string> _termesDeclencheurs = new List<string>
    {
        "Hémoglobine A1C", "Microalbumine", "Taille", "Poids",
        "Fumeur", "Fumeuse", "Anormal", "Cholestérol",
        "Vertiges", "Rechute", "Réaction", "Anticorps"
    };

        public ReportService(IPatientService patientService, INoteService noteService)
        {

            _patientService = patientService;
            _noteService = noteService;

        }

        //Calcule le risque de diabète en fonction des notes (MongoDB),
        // compare les notes avec la liste des mots déclencheurs sans tenir compte de la casse et les variations des mots. 
        private async Task<int> CountRiskNoteAsync(int patientId)
        {
            List<Note> notes = await _noteService.GetPatientByPatIdAsync(patientId);
            int countNote = notes.Sum(note =>
                _termesDeclencheurs.Count(term =>
                    Regex.IsMatch(note.NoteText, $@"\b{term}\w*\b", RegexOptions.IgnoreCase)
                )
            );

            return countNote;
        }

        //Calcule l'âge du patient
        private async Task<int> CalculateAge(int patientId)
        {
            DateTime dateTime = DateTime.Now;
            Patient patient = await _patientService.GetPatientByIdAsync(patientId);
            DateTime dateOfBirthday = patient.DateOfBirth;

            int age = dateTime.Year - dateOfBirthday.Year;
            if (dateOfBirthday > dateTime.AddYears(-age))
            {
                age--;
            }

            return age; // Fallback
        }

        //Calcule le risque de diabete
        public async Task<string> CalculateAssessmentDiabetePatient(int patientId)
        {
            Patient patient = await _patientService.GetPatientByIdAsync(patientId);
            int age = await CalculateAge(patientId);
            int triggerCount = await CountRiskNoteAsync(patientId);

            if (triggerCount == 0) return "None";

            if (triggerCount >= 2 && triggerCount <= 5 && age > 30)
                return "Borderline";

            if (age <= 30)
            {
                if (patient.Gender == "Male")
                {
                    if (triggerCount >= 3 && triggerCount < 5) return "In Danger";
                    if (triggerCount >= 5) return "Early onset";
                }
                else if (patient.Gender == "Female")
                {
                    if (triggerCount >= 4 && triggerCount < 7) return "In Danger";
                    if (triggerCount >= 7) return "Early onset";
                }
                else
                {

                }
            }
            else // Age > 30
            {
                if (triggerCount == 6 || triggerCount == 7) return "In Danger";
                if (triggerCount >= 8) return "Early onset";
            }

            return "None"; // Fallback
        }
    }
}

