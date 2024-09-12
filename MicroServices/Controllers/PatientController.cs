using MicroServicePatient.Data;
using MicroServicePatient.Models;
using MicroServicePatient.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MicroServicePatient.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly PatientDbContext _context;

        public PatientController(IPatientService PatientRepository, ILogger<PatientController> logger)
        {
            _patientService = PatientRepository;
        }

        // Impl�mentez l'API RESTFUL pour cr�er une entit� Patient dans le DataRepository
        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient Patient)
        {
            try
            {
                var createdPatient = await _patientService.CreatePatientAsync(Patient);
                return Ok(Patient);
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while creating a Patient");
            }
        }

        // Impl�mentez l'API RESTFUL pour r�cup�rer une entit� Patient
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            try
            {
                var Patient = await _patientService.GetPatientByIdAsync(id);

                return Ok(Patient);
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while getting a Patient");
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetPatientByNameDob(string firstName, string lastName, DateTime dateOfBirth)
        {
            var patient = await _patientService.GetPatientByNameDob(firstName, lastName, dateOfBirth);

            return Ok(patient);
        }
        // Impl�mentez l'API RESTFUL pour modifier une entit� Patient
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] Patient Patient)
        {
            if (Patient == null || Patient.Id != id)
            {

                return BadRequest("Patient object is null or ID mismatch");
            }

            if (!ModelState.IsValid)
            {

                return BadRequest(ModelState);  // Renvoie les erreurs de validation des donn�es
            }

            try
            {
                var updatedPatient = await _patientService.UpdatePatientAsync(Patient);
                if (updatedPatient == null)
                {

                    return NotFound();
                }


                return Ok(updatedPatient);
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while updating a Patient");
            }
        }

        // Impl�mentez l'API RESTFUL pour supprimer une entit� Patient
        [HttpDelete("{id}")]

        public async Task<IActionResult> DeletePatient(int id)
        {

            try
            {
                var result = await _patientService.DeletePatientAsync(id);

                return NoContent();
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while deleting a Patient");
            }
        }
        // Récupération de tous les Patient
        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            try
            {
                var Patients = await _patientService.GetAllPatientsAsync();
                if (Patients == null || !Patients.Any())
                {
                    return NotFound();
                }
                return Ok(Patients);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while retrieving all Patients");
            }
        }
    }
}