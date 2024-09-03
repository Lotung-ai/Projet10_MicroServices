namespace MicroFrontEnd.Models
{
    public class PatientNote
    {
        // Informations du patient (SQL)
        public PatientViewModel Patient { get; set; }

        // Liste des notes du patient (MongoDB)
        public NoteViewModel NoteText { get; set; }

        public PatientNote()
        {
            Patient = new PatientViewModel();
            NoteText = new NoteViewModel();
        }
    }
}
