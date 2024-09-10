namespace MicroServiceNote.Data
{
    public class PatientDbSettings
    {//Settings for MongoDB
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string PatientsCollectionName { get; set; } = null!;
    }

}
