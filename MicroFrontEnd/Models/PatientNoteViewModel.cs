namespace MicroFrontEnd.Models
{
    public class PatientNoteViewModel
    {
        // Informations du patient (SQL)
        public PatientViewModel Patient { get; set; }

        // Liste des notes du patient (MongoDB)
        public List<NoteViewModel> Notes { get; set; }

        public string RiskDiabete { get; set; }

        public PatientNoteViewModel()
        {
            Patient = new PatientViewModel();
            Notes = new List<NoteViewModel>();
            RiskDiabete = string.Empty;
        }
    }
}
