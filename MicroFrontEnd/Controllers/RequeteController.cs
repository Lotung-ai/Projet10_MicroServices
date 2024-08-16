using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using MicroServices.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Logging;

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

        [HttpGet]
        public async Task<IActionResult> PatientDetails(int patientId)
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

        [HttpPost]
        public async Task<IActionResult> PatientManagement(Patient patient)
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
                        return RedirectToAction("Index", "Home");
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

            return View("/Views/Home/PatientMangement.cshtml", patient);
        }
    }
}
