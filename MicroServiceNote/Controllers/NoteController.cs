using MicroServiceNote.Models;
using MicroServiceNote.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace MicroServiceNote.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/[controller]")]
    public class NoteController : ControllerBase
    {
        private readonly NoteService _noteService;

        public NoteController(NoteService noteService) =>
            _noteService = noteService;

        [HttpGet]
        public async Task<List<Note>> Get() =>
            await _noteService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Note>> Get(string id)
        {
            var patient = await _noteService.GetAsync(id);

            if (patient is null)
            {
                return NotFound();
            }

            return patient;
        }
        [HttpGet("{patId}")]
        public async Task<ActionResult<Note>> GetPatientByPatId(int patId)
        {
            var patient = await _noteService.GetPatientByPatIdAsync(patId);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
        [HttpPost]
        public async Task<IActionResult> Post(Note newPatient)
        {
            await _noteService.CreateAsync(newPatient);

            return CreatedAtAction(nameof(Get), new { id = newPatient.Id }, newPatient);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Note updatedPatient)
        {
            var patient = await _noteService.GetAsync(id);

            if (patient is null)
            {
                return NotFound();
            }

            updatedPatient.Id = patient.Id;

            await _noteService.UpdateAsync(id, updatedPatient);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var patient = await _noteService.GetAsync(id);

            if (patient is null)
            {
                return NotFound();
            }

            await _noteService.RemoveAsync(id);

            return NoContent();
        }
        [HttpDelete("{patId}")]
        public async Task<ActionResult<Note>> DeletePatientByPatId(int patId)
        {
            try
            {
                await _noteService.DeletePatientByPatIdAsync(patId);
                return Ok();
            }

            catch
            {
                return NotFound();
            }

        }
    }
}