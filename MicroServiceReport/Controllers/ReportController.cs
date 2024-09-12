using MicroServiceReport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MicroServiceReport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetDiabetesAssessment(int patientId)
        {
            var assessment = await _reportService.CalculateAssessmentDiabetePatient(patientId);

            if (assessment == null)
            {
                return NotFound("Patient not found or no assessment available");
            }

            return Ok(assessment);
        }
    }
}
