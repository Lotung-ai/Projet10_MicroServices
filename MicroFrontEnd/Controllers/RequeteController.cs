using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using MicroServices.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Expressions;

namespace MicroFrontEnd.Controllers
{
    public class RequeteController : Controller
    {
        private readonly ILogger<RequeteController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://localhost:5003/gateway/patients"; // URL Ocelot Gateway

        public RequeteController(IHttpClientFactory httpClientFactory, ILogger<RequeteController> logger)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient(); // Créer le client ici
        }

        //Methde Get pour afficher tous les patients
        [HttpGet]
        public async Task<IActionResult> PatientManagement()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var patients = JsonSerializer.Deserialize<List<Patient>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View("/Views/Home/PatientManagement.cshtml",patients); // Assurez-vous que le nom de la vue est correct
                }
                else
                {
                    _logger.LogError("Unable to retrieve patients from API.");
                    return View("/Views/Home/PatientManagement.cshtml", new List<Patient>()); // Retourne une vue avec une liste vide
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients.");
                return View("/Views/Home/PatientManagement.cshtml", new List<Patient>()); // Retourne une vue avec une liste vide
            }

        }

        //Méthode Create pour créer un patient
        public async Task<IActionResult> PostPatientCreate(Patient patient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var json = JsonSerializer.Serialize(patient);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync(_apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Stocker un message de succès dans ViewData
                        ViewData["SuccessMessage"] = "Patient created successfully!";
                        return View("/Views/Home/PatientCreate.cshtml");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to create patient. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating patient.");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            return View("/Views/Home/PatientCreate.cshtml", patient);
        }

        //Méthode Get pour avoir les détails
        public async Task<IActionResult> GetPatientDetails(int patientId)
        {
            if (patientId <= 0)
            {
                ViewData["ErrorMessage"] = "Invalid patient ID.";
                return View("/Views/Home/PatientDetails.cshtml"); // Nom de la vue sans chemin complet
            }

            try
            {
                // Envoie une requête GET à l'API via Ocelot
                var response = await _httpClient.GetAsync($"{_apiUrl}/{patientId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var patient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Stocker le patient récupéré dans ViewData pour l'envoyer à la vue
                    return View("/Views/Home/PatientDetails.cshtml", patient); // Passer le modèle directement à la vue
                }
                else
                {
                    ViewData["ErrorMessage"] = "Patient not found or an error occurred.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patient.");
                ViewData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
            }

            // Retourner la vue avec un modèle null si aucune donnée n'est trouvée
            return View("/Views/Home/PatientDetails.cshtml"); // Nom de la vue sans chemin complet
        }

        // Méthode GET pour mettre à jour un patient
        public async Task<IActionResult> GetPatientUpdate(int patientId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/{patientId}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var patient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return View("/Views/Home/PatientUpdate.cshtml", patient);
                }
                else
                {
                    _logger.LogError("Unable to retrieve patients from API.");
                    return RedirectToAction("PatientManagement");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving patients.");
                return RedirectToAction("PatientManagement");
            }
        }

        // Méthode PUT pour mettre à jour un patient
        public async Task<IActionResult> UpdatePatient(Patient patient)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var json = JsonSerializer.Serialize(patient);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Envoyer une requête PUT pour mettre à jour le patient
                    var response = await _httpClient.PutAsync($"{_apiUrl}/{patient.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        ViewData["SuccessMessage"] = "Patient successfully updated!";
                        return RedirectToAction("PatientManagement");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to update patient. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating patient.");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            // Si une erreur survient, retourner la vue avec les données actuelles du patient
            return View("/Views/Home/PatientUpdate.cshtml", patient);
        }

        // Méthode DELETE pour supprimer un patient
        public async Task<IActionResult> DeletePatient(int patientId)
        {
            if (patientId <= 0)
            {
                ModelState.AddModelError("", "Invalid patient ID.");
                return RedirectToAction("PatientManagement");
            }

            try
            {
                // Appel à la passerelle API via Ocelot
                var response = await _httpClient.DeleteAsync($"{_apiUrl}/{patientId}");

                if (response.IsSuccessStatusCode)
                {
                    ViewData["SuccessMessage"] = "Patient deleted successfully.";
                    return RedirectToAction("PatientManagement");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to delete patient. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting patient.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
            }

            return RedirectToAction("PatientManagement");
        }

    }
}


