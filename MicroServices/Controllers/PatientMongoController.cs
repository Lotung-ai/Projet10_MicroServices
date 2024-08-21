using MicroServices.Models;
using MicroServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace MicroServices.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientMongoController : ControllerBase
    {
        private readonly PatientMongoService _patientService;

        public PatientMongoController(PatientMongoService patientService) =>
            _patientService = patientService;

        [HttpGet]
        public async Task<List<PatientMongo>> Get() =>
            await _patientService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<PatientMongo>> Get(string id)
        {
            var patient = await _patientService.GetAsync(id);

            if (patient is null)
            {
                return NotFound();
            }

            return patient;
        }
        [HttpGet("{patId}")]
        public async Task<ActionResult<PatientMongo>> GetPatientByPatId(int patId)
        {
            var patient = await _patientService.GetPatientByPatIdAsync(patId);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
        [HttpPost]
        public async Task<IActionResult> Post(PatientMongo newPatient)
        {
            await _patientService.CreateAsync(newPatient);

            return CreatedAtAction(nameof(Get), new { id = newPatient.Id }, newPatient);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, PatientMongo updatedPatient)
        {
            var patient = await _patientService.GetAsync(id);

            if (patient is null)
            {
                return NotFound();
            }

            updatedPatient.Id = patient.Id;

            await _patientService.UpdateAsync(id, updatedPatient);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var patient = await _patientService.GetAsync(id);

            if (patient is null)
            {
                return NotFound();
            }

            await _patientService.RemoveAsync(id);

            return NoContent();
        }
    }
}