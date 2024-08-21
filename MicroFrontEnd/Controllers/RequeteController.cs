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

namespace MicroFrontEnd.Controllers
{
    public class RequeteController : Controller
    {
        private readonly ILogger<RequeteController> _logger;
        private readonly IFrontService _frontService;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrlSQL = "http://ocelotapigw:80/gateway/patients";
        private readonly string _apiUrlMongo = "http://ocelotapigw:80/gateway/patientsmongo";

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
                var SqlResponse = await _httpClient.GetAsync(_apiUrlSQL);

                if (SqlResponse.IsSuccessStatusCode)
                {
                    var json = await SqlResponse.Content.ReadAsStringAsync();
                    var sqlPatients = JsonSerializer.Deserialize<List<Patient>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View("/Views/Home/PatientManagement.cshtml", sqlPatients); // Assurez-vous que le nom de la vue est correct
                }
                else
                {
                    _logger.LogError("Unable to retrieve sqlPatients from API.");
                    return View("/Views/Home/PatientManagement.cshtml", new List<Patient>()); // Retourne une vue avec une liste vide
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sqlPatients.");
                return View("/Views/Home/PatientManagement.cshtml", new List<Patient>()); // Retourne une vue avec une liste vide
            }

        }

        [HttpPost]
        //Méthode Create pour créer un sqlPatient
        public async Task<IActionResult> PostPatientCreate(PatientViewModel sqlPatient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var patient = _frontService.MapFrontEndToPatientApi(sqlPatient);
                    var json = JsonSerializer.Serialize(patient);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var SqlResponse = await _httpClient.PostAsync(_apiUrlSQL, content);

                    if (SqlResponse.IsSuccessStatusCode)
                    {
                        // Stocker un message de succès dans ViewData
                        _logger.LogTrace("Patient created successfully");
                        return Ok();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to create sqlPatient. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating sqlPatient.");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            return BadRequest(ModelState);
        }
        [HttpPost]
        public async Task<IActionResult> PostNoteCreate(NoteViewModel noteView)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var note = _frontService.MapFrontEndToNoteApi(noteView);
                    var json = JsonSerializer.Serialize(note);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var mongoResponse = await _httpClient.PostAsync(_apiUrlMongo, content);

                    if (mongoResponse.IsSuccessStatusCode)
                    {
                        _logger.LogTrace("Note created successfully");
                        return Ok(); // Retourne OK si l'ajout est réussi
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to create Note. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating Note.");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            return BadRequest(ModelState); // Retourne BadRequest en cas d'échec de validation
        }

        [HttpPost]
        public async Task<IActionResult> PostPatientNoteCreate(PatientNote patientNote)
        {
            try
            {
                
                var sql = JsonSerializer.Serialize(patientNote.Patient);
                var sqlcontent = new StringContent(sql, Encoding.UTF8, "application/json");

                var SqlResponse = await _httpClient.PostAsync(_apiUrlSQL, sqlcontent);

                if (SqlResponse.IsSuccessStatusCode)
                {
                    // Stocker un message de succès dans ViewData
                    _logger.LogTrace("Patient created successfully");                   
                }
                else
                {
                    ModelState.AddModelError("", "Unable to create sqlPatient. Please try again.");
                }

                //DONE Mettre en relation Id patient et Id Note v
                var sqlResponse = await _httpClient.GetAsync($"{_apiUrlSQL}/search?firstName={patientNote.Patient.FirstName}&lastName={patientNote.Patient.LastName}&dateOfBirth={patientNote.Patient.DateOfBirth.Year}-{patientNote.Patient.DateOfBirth.Month}-{patientNote.Patient.DateOfBirth.Day}");

                var json = await sqlResponse.Content.ReadAsStringAsync();
                var sqlPatient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var mongoPatient = new NoteViewModel
                {
                    PatId = sqlPatient.Id,
                    Patient = patientNote.Patient.FirstName,
                    Note = patientNote.Note.Note
                };
                var mongo = JsonSerializer.Serialize(mongoPatient);
                var mongocontent = new StringContent(mongo, Encoding.UTF8, "application/json");

                var mongoResponse = await _httpClient.PostAsync(_apiUrlMongo, mongocontent);

                if (mongoResponse.IsSuccessStatusCode)
                {
                    _logger.LogTrace("Note created successfully");
                    // Retourne OK si l'ajout est réussi
                }
                else
                {
                    ModelState.AddModelError("", "Unable to create Note. Please try again.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating sqlPatient.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return View("/Views/Home/PatientCreate.cshtml", patientNote);
        }

        //Méthode Get pour avoir les données SQL
        private async Task<PatientNoteViewModel> GetPatientSQLDataAsync(int patientId)
        {
            PatientNoteViewModel patientNoteViewModel = new PatientNoteViewModel();

            try
            {
                // Récupérer les données SQL
                var sqlResponse = await _httpClient.GetAsync($"{_apiUrlSQL}/{patientId}");
                if (sqlResponse.IsSuccessStatusCode)
                {
                    var json = await sqlResponse.Content.ReadAsStringAsync();
                    var sqlPatient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Construire le modèle avec les données du patient SQL
                    patientNoteViewModel.Patient.Id = sqlPatient.Id;
                    patientNoteViewModel.Patient.FirstName = sqlPatient.FirstName;
                    patientNoteViewModel.Patient.LastName = sqlPatient.LastName;
                    patientNoteViewModel.Patient.DateOfBirth = sqlPatient.DateOfBirth;
                    patientNoteViewModel.Patient.Gender = sqlPatient.Gender;
                    patientNoteViewModel.Patient.Address = sqlPatient.Address;
                    patientNoteViewModel.Patient.PhoneNumber = sqlPatient.PhoneNumber;
                }
                else
                {
                    _logger.LogError($"Failed to fetch patient from SQL DB. Status Code: {sqlResponse.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patient data.");
            }

            return patientNoteViewModel;
        }
        //Méthode Get pour avoir les données MongoDB
        private async Task<PatientNoteViewModel> GetPatientMongoDataAsync(int patientId)
        {
            PatientNoteViewModel patientNoteViewModel = new PatientNoteViewModel();

            try
            {

                // Récupérer les données MongoDB (notes)
                var mongoResponse = await _httpClient.GetAsync($"{_apiUrlMongo}/bypatid/{patientId}");
                if (mongoResponse.IsSuccessStatusCode)
                {
                    var mongoJson = await mongoResponse.Content.ReadAsStringAsync();
                    var mongoPatients = JsonSerializer.Deserialize<List<PatientMongo>>(mongoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                    // Ajouter les notes au modèle
                    foreach (var mongoPatient in mongoPatients)
                    {
                        var note = new NoteViewModel
                        {
                            Id = mongoPatient.Id,
                            PatId = mongoPatient.PatId,
                            Patient = mongoPatient.Patient,
                            Note = mongoPatient.Note
                        };

                        patientNoteViewModel.Notes.Add(note); // Ajouter chaque note à la liste
                    }
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patient data.");
            }

            return patientNoteViewModel;
        }
        //Méthode Get pour avoir les données SQL et MongoDB
        private async Task<PatientNoteViewModel> GetPatientDataAsync(int patientId)
        {
            PatientNoteViewModel patientNoteViewModel = new PatientNoteViewModel();

            try
            {              
                    // Récupérer les données SQL
                    var sqlResponse = await _httpClient.GetAsync($"{_apiUrlSQL}/{patientId}");
                    if (sqlResponse.IsSuccessStatusCode)
                    {
                        var json = await sqlResponse.Content.ReadAsStringAsync();
                        var sqlPatient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        // Construire le modèle avec les données du patient SQL
                        patientNoteViewModel.Patient.Id = sqlPatient.Id;
                        patientNoteViewModel.Patient.FirstName = sqlPatient.FirstName;
                        patientNoteViewModel.Patient.LastName = sqlPatient.LastName;
                        patientNoteViewModel.Patient.DateOfBirth = sqlPatient.DateOfBirth;
                        patientNoteViewModel.Patient.Gender = sqlPatient.Gender;
                        patientNoteViewModel.Patient.Address = sqlPatient.Address;
                        patientNoteViewModel.Patient.PhoneNumber = sqlPatient.PhoneNumber;
                    }
                    else
                    {
                        _logger.LogError($"Failed to fetch patient from SQL DB. Status Code: {sqlResponse.StatusCode}");
                    }                              

                // Récupérer les données MongoDB (notes)
                var mongoResponse = await _httpClient.GetAsync($"{_apiUrlMongo}/bypatid/{patientId}");
                if (mongoResponse.IsSuccessStatusCode)
                {
                    var mongoJson = await mongoResponse.Content.ReadAsStringAsync();
                    var mongoPatients = JsonSerializer.Deserialize<List<PatientMongo>>(mongoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                    // Ajouter les notes au modèle
                    foreach (var mongoPatient in mongoPatients)
                    {
                        var note = new NoteViewModel
                        {
                            Id = mongoPatient.Id,
                            PatId = mongoPatient.PatId,
                            Patient = mongoPatient.Patient,
                            Note = mongoPatient.Note
                        };

                        patientNoteViewModel.Notes.Add(note); // Ajouter chaque note à la liste
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patient data.");
            }

            return patientNoteViewModel;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            if (patientId <= 0)
            {
                ViewData["ErrorMessage"] = "Invalid Patient ID.";
                return View("/Views/Home/PatientDetails.cshtml");
            }

            var patientViewModel = await GetPatientDataAsync(patientId);

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

            var patientViewModel = await GetPatientDataAsync(patientId);

            if (patientViewModel == null)
            {
                ViewData["ErrorMessage"] = "Patient not found or an error occurred.";
                return View("/Views/Home/PatientManagement.cshtml");
            }

            return View("/Views/Home/PatientUpdate.cshtml", patientViewModel);
        }

        // Méthode POST pour mettre à jour un sqlPatient
        [HttpPut]
        private async Task<bool> UpdatePatientAsync(PatientViewModel sqlPatient)
        {
            try
            {
                _logger.LogInformation("Starting update for SQL Patient ID: {PatientId}", sqlPatient.Id);

                var json = JsonSerializer.Serialize(sqlPatient);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Envoyer une requête PUT pour mettre à jour le patient
                var sqlResponse = await _httpClient.PutAsync($"{_apiUrlSQL}/{sqlPatient.Id}", content);

                if (sqlResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully updated SQL Patient ID: {PatientId}", sqlPatient.Id);
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to update SQL Patient ID: {PatientId}. Status Code: {StatusCode}", sqlPatient.Id, sqlResponse.StatusCode);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating SQL Patient ID: {PatientId}", sqlPatient.Id);
                return false;
            }
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

        [HttpPut]
        public async Task<IActionResult> UpdatePatientDataAsync(PatientNoteViewModel updatedPatientNoteViewModel)
        {
            bool updateSuccessful = true;

            // Mise à jour des données SQL (patient)
            bool sqlUpdateResult = await UpdatePatientAsync(updatedPatientNoteViewModel.Patient);
            if (!sqlUpdateResult)
            {
                updateSuccessful = false;
            }

            // Mise à jour des données MongoDB (notes)
            foreach (var note in updatedPatientNoteViewModel.Notes)
            {
                bool notesUpdateResult = await UpdateNotesAsync(note);
                if (!notesUpdateResult)
                {
                    updateSuccessful = false;
                }
            }
            if (!updateSuccessful)
            {
                ViewData["ErrorMessage"] = "Some updates failed. Please try again.";
                return View("/Views/Home/PatientUpdate.cshtml", updatedPatientNoteViewModel); // Retourner la vue avec les erreurs
            }

            ViewData["SuccessMessage"] = "Patient and notes updated successfully!";
            return View("/Views/Home/PatientUpdate.cshtml", updatedPatientNoteViewModel);
        }


        // Méthode DELETE pour supprimer un sqlPatient
        public async Task<IActionResult> DeletePatient(int sqlPatientId)
        {
            if (sqlPatientId <= 0)
            {
                ModelState.AddModelError("", "Invalid sqlPatient ID.");
                return RedirectToAction("PatientManagement");
            }

            try
            {
                // Appel à la passerelle API via Ocelot
                var SqlResponse = await _httpClient.DeleteAsync($"{_apiUrlSQL}/{sqlPatientId}");

                if (SqlResponse.IsSuccessStatusCode)
                {
                    ViewData["SuccessMessage"] = "Patient deleted successfully.";
                    return RedirectToAction("PatientManagement");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete sqlPatient. Please try again.");
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


