using MicroFrontEnd.Models;
using MicroServicePatient.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiUrlLogin = "http://ocelotapigw:80/gateway/register";

        public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Créer la requête JSON
            var loginData = new Login
            {
                UserName = model.UserName,
                Password = model.Password,
                RememberMe = model.RememberMe
            };

            var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");

            // Envoyer la requête à Ocelot qui la relayera à l'API
            var response = await _httpClient.PostAsync($"{_apiUrlLogin}/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<ApiResponseModel>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Log the content
                _logger.LogInformation("Response Content: {ResponseContent}", responseContent);
                _logger.LogInformation("Token: {Token}", responseObject?.Token);
                _logger.LogInformation("Message: {Message}", responseObject?.Message);

                // Vérifier que le jeton n'est pas null ou vide
                if (!string.IsNullOrEmpty(responseObject?.Token))
                {
                    model.JwtToken = responseObject.Token;

                    // Stocker le jeton dans les cookies
                    Response.Cookies.Append("jwt", model.JwtToken, new CookieOptions
                    {
                        HttpOnly = true,                       
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddHours(2)
                    });
                    ViewData["LoginMessage"] = "Login Success";
                    return View("/Views/Home/Index.cshtml");
                }
                else
                {
                    ViewData["ErrorMessage"] = "Invalid login attempt. No token received.";
                    return View("/Views/Account/Index.cshtml", model);
                }
            }

            ViewData["ErrorMessage"] = "UserName or Password format invalid";
            // Si on arrive ici, c'est que quelque chose s'est mal passé, on réaffiche le formulaire
            return View("/Views/Account/Index.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Envoyer une requête de déconnexion à l'API
            var response = await _httpClient.PostAsync($"{_apiUrlLogin}/logout", null);

            // Supprimer le cookie JWT pour déconnecter l'utilisateur localement
            Response.Cookies.Delete("jwt");

            if (response.IsSuccessStatusCode)
            {
                // Extraire le message de la réponse (optionnel)
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Logout successful. Response: {ResponseContent}", responseContent);

                ViewData["LoginMessage"] = "Logout Success";

                // Rediriger l'utilisateur vers la page d'accueil après la déconnexion réussie
                return View("/Views/Home/Index.cshtml");
            }

            // Loguer l'erreur en cas de problème lors de la déconnexion
            _logger.LogWarning("Logout failed. StatusCode: {StatusCode}", response.StatusCode);
            ViewData["ErrorMessage"] = "An error occurred during logout.";

            // Rediriger l'utilisateur vers la page d'accueil même en cas d'erreur, après suppression du cookie
            return View("/Views/Home/Index.cshtml");
        }
    }
}

