namespace MicroServiceReport.Services.Interfaces
{
    public interface IReportService
    {
        public Task<string> CalculateAssessmentDiabetePatient(int patientId);
    }
}