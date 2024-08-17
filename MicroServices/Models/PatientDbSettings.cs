
namespace MicroServices.Models
{
    public class PatientDbSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string PatientsCollectionName { get; set; } = null!;
    }

}
