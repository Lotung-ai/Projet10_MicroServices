using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using MicroServiceNote.Models;
using MicroFrontEnd.Models;
using MicroFrontEnd.Services.Interfaces;
using System.Net.Http.Headers;
using MicroServicePatient.Models;

namespace MicroFrontEnd.Controllers
{
    public class RequeteController : Controller
    {
        private readonly ILogger<RequeteController> _logger;
        private readonly IFrontService _frontService;
        private readonly HttpClient _httpClient;

        public RequeteController(IHttpClientFactory httpClientFactory, ILogger<RequeteController> logger, IFrontService frontservice)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient(); // Créer le client ici
            _frontService = frontservice;
        }

        //Methde Get pour afficher tous les sqlPatients
        [HttpGet]
        public async Task<IActionResult> PatientManagement()
        {
            try
            {
                List<PatientNoteViewModel> patientNoteViewModel = await _frontService.GetPatientManagement();

                return View("/Views/Home/PatientManagement.cshtml", patientNoteViewModel); // Assurez-vous que le nom de la vue est correct

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sqlPatients.");
                return View("/Views/Home/PatientManagement.cshtml", new List<PatientNoteViewModel>()); // Retourne une vue avec une liste vide
            }

        }

        [HttpPost]
        public async Task<IActionResult> PostPatientNoteCreate(PatientNote patientNote)
        {

            try
            {
                //Post SQL data patient
                await _frontService.PostPatientCreate(patientNote.Patient);
                //Post Mongo data note
                await _frontService.PostNoteCreate(patientNote);
                _logger.LogInformation("Patient and notes created successfully.");
                ViewData["SuccessMessage"] = "Patient and notes created successfully.";
                return RedirectToAction("PatientManagement");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating Patient.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                ViewData["ErrorMessage"] = "Patient and notes created failed.";
                return View("/Views/Home/PatientCreate.cshtml", patientNote);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            if (patientId <= 0)
            {
                ViewData["ErrorMessage"] = "Invalid Patient ID.";
                return View("/Views/Home/PatientDetails.cshtml");
            }

            var patientViewModel = await _frontService.GetPatientDataAsync(patientId);

            if (patientViewModel == null)
            {
                ViewData["ErrorMessage"] = "Patient not found or an error occurred.";
                return View("/Views/Home/PatientDetails.cshtml");
            }

            return View("/Views/Home/PatientDetails.cshtml", patientViewModel);
        }

        // Méthode GET pour mettre à jour un sqlPatient
        [HttpGet]
        public async Task<IActionResult> GetPatientUpdate(int patientId)
        {
            if (patientId <= 0)
            {
                ViewData["ErrorMessage"] = "Invalid Patient ID.";
                return View("/Views/Home/PatientManagement.cshtml");
            }

            var patientViewModel = await _frontService.GetPatientDataAsync(patientId);

            if (patientViewModel == null)
            {
                ViewData["ErrorMessage"] = "Patient not found or an error occurred.";
                return View("/Views/Home/PatientManagement.cshtml");
            }

            return View("/Views/Home/PatientUpdate.cshtml", patientViewModel);
        }

        //Méthode pour update les données patient et ajouter une note
        [HttpPost]
        public async Task<IActionResult> UpdatePatientNoteData(PatientNoteViewModel updatedPatientNoteViewModel, string NewNote)
        {
            try
            {
                try
                {
                    await _frontService.UpdatePatientData(updatedPatientNoteViewModel.Patient);
                }
                catch
                {
                    _logger.LogError($"Failed to update SQL Patient. Status Code");
                    ViewData["ErrorMessage"] = "Failed to update patient information.";
                    return View("/Views/Home/PatientUpdate.cshtml", updatedPatientNoteViewModel);
                }
                // Mise à jour des notes dans MongoDB  
                try
                {
                    _logger.LogInformation("Try to update notes: " + updatedPatientNoteViewModel.Notes.Count);
                    foreach (NoteViewModel note in updatedPatientNoteViewModel.Notes)
                    {
                        await _frontService.UpdateNoteData(note);
                    }
                    _logger.LogInformation("Successfully updates notes");

                    // Si une nouvelle note est fournie, créez-la
                    if (!string.IsNullOrEmpty(NewNote))
                    {
                        var newPatientNote = new PatientNote
                        {
                            Patient = updatedPatientNoteViewModel.Patient,
                            NoteText = new NoteViewModel { Note = NewNote }
                        };
                        await _frontService.PostNoteCreate(newPatientNote);
                    }
                }
                catch
                {
                    _logger.LogError($"Failed to update MongoDB note. Status Code");
                    ViewData["ErrorMessage"] = "Failed to update one or more notes.";
                    return View("/Views/Home/PatientUpdate.cshtml", updatedPatientNoteViewModel);
                }

                _logger.LogInformation("Successfully updated patient and notes.");
                ViewData["SuccessMessage"] = "Patient and notes updated successfully.";

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating patient and notes.");
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
            }

            return View("/Views/Home/PatientUpdate.cshtml", updatedPatientNoteViewModel);
        }

        // Méthode DELETE pour supprimer un sqlPatient        
        public async Task<IActionResult> DeletePatient(int patientId)
        {
            if (patientId <= 0)
            {
                ModelState.AddModelError("", "Invalid sqlPatient ID.");
                return RedirectToAction("PatientManagement");
            }

            try
            {
                // Appel à la passerelle API via Ocelot
                await _frontService.DeletePatientAndNote(patientId);

                ViewData["SuccessMessage"] = "Patient deleted successfully.";

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting sqlPatient.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return RedirectToAction("PatientManagement");
        }

    }
}


