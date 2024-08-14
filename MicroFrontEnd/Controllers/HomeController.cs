using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MicroFrontEnd.Models;
using System.Net.Http;
using System.Text.Json;
using MicroServices.Models;
using System.Text;


namespace MicroFrontEnd.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "http://localhost:5003/Patient"; // URL de l'API microservice via Ocelot Gateway

        public HomeController(HttpClient httpClient, ILogger<HomeController> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult PatientManagement()
        {
            return View();
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
                        // Rediriger vers une page de confirmation ou une liste de patients
                        return RedirectToAction("Index");
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

            return View(patient);
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
