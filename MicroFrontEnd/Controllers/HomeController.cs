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

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public IActionResult PatientDetails()
        {
            return View();
        }       

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
