using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using MicroServices.Models;
using MicroFrontEnd.Models;
using MicroFrontEnd.Services;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Expressions;
using MicroFrontEnd.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.AspNetCore.Authorization;

namespace MicroFrontEnd.Controllers
{
    public class RequeteController : Controller
    {
        private readonly ILogger<RequeteController> _logger;
        private readonly IFrontService _frontService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrlSQL = "http://ocelotapigw:80/gateway/patients";
        private readonly string _apiUrlMongo = "http://ocelotapigw:80/gateway/notemongo";

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
                var patientNoteViewModels = new List<PatientNoteViewModel>();
                var SqlResponse = await _httpClient.GetAsync(_apiUrlSQL);

                if (SqlResponse.IsSuccessStatusCode)
                {
                    var json = await SqlResponse.Content.ReadAsStringAsync();
                    var sqlPatients = JsonSerializer.Deserialize<List<Patient>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    foreach (Patient patient in sqlPatients)
                    {
                        var patientNoteViewModel = new PatientNoteViewModel();
                        patientNoteViewModel.Patient = _frontService.MaPatientApiToPatientViewModel(patient);
                        patientNoteViewModel.RiskDiabete = await _frontService.CalculateAssessmentDiabetePatient(patient.Id);
                        patientNoteViewModels.Add(patientNoteViewModel);
                    }
                        return View("/Views/Home/PatientManagement.cshtml", patientNoteViewModels); // Assurez-vous que le nom de la vue est correct
                }
                else
                {
                    _logger.LogError("Unable to retrieve sqlPatients from API.");
                    return View("/Views/Home/PatientManagement.cshtml", new List<PatientNoteViewModel>()); // Retourne une vue avec une liste vide
                }
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
                ViewData["SuccessMessage"] = "Patient and notes created successfully.";
                return RedirectToAction("PatientManagement", "Requete");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating Patient.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                return View("/View/Requete/PatientCreate.cshtml", patientNote);

            }
            
            return View("/Views/Home/PatientCreate.cshtml", patientNote);
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

        // Méthode POST pour mettre à jour les notes
        [HttpPut]       
        private async Task<bool> UpdateNotesAsync(NoteViewModel note)
        {
            bool allUpdatesSuccessful = true;

            try
            {
                var mongoNoteUpdateUrl = $"{_apiUrlMongo}/byid/{note.Id}";
                _logger.LogInformation("Updating MongoDB note data at URL: {MongoNoteUpdateUrl}", mongoNoteUpdateUrl);

                var mongoNoteContent = new StringContent(JsonSerializer.Serialize(note), Encoding.UTF8, "application/json");
                var mongoResponse = await _httpClient.PutAsync(mongoNoteUpdateUrl, mongoNoteContent);

                if (mongoResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated MongoDB note ID: {NoteId}", note.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to update MongoDB note ID: {NoteId}. Status Code: {StatusCode}", note.Id, mongoResponse.StatusCode);
                    allUpdatesSuccessful = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating MongoDB note ID: {NoteId}", note.Id);
                allUpdatesSuccessful = false;
            }


            return allUpdatesSuccessful;
        }

        //TODO Utiliser un Get et un Post
        [HttpPost]
         public async Task<IActionResult> UpdatePatientNoteData(PatientNoteViewModel updatedPatientNoteViewModel)
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
                var SqlResponse = await _httpClient.DeleteAsync($"{_apiUrlSQL}/{patientId}");

                if (SqlResponse.IsSuccessStatusCode)
                {
                    ViewData["SuccessMessage"] = "Patient deleted successfully.";
                    
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete sqlPatient. Please try again.");
                }

                var MongoResponse = await _httpClient.DeleteAsync($"{_apiUrlMongo}/bypatid/{patientId}");

                if (MongoResponse.IsSuccessStatusCode)
                {
                    ViewData["SuccessMessage"] = "Note deleted successfully.";
                    
                }
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


