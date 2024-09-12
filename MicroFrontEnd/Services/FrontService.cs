using MicroFrontEnd.Models;
using MicroFrontEnd.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MicroFrontEnd.Services
{
    public class FrontService : IFrontService
    {
        private readonly IDto _dto;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrontService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiUrlSQL = "http://ocelotapigw:80/gateway/patients";
        private readonly string _apiUrlMongo = "http://ocelotapigw:80/gateway/notemongo";
        private readonly string _apiUrlReport = "http://ocelotapigw:80/gateway/report";

        public FrontService(IDto dto, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, ILogger<FrontService> logger)
        {
            _dto = dto;
            _httpClient = httpClientFactory.CreateClient();
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private string GetJwtTokenFromCookies()
        {
            // Utilisation de l'IHttpContextAccessor pour récupérer le cookie JWT
            var token = _httpContextAccessor.HttpContext?.Request.Cookies["jwt"];
            return token ?? string.Empty;
        }

        // Méthode pour ajouter le token dans l'en-tête Authorization
        private void AddJwtTokenToRequestHeaders()
        {
            var token = GetJwtTokenFromCookies();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        //Méthode Create pour créer un sqlPatient
        public async Task PostPatientCreate(PatientViewModel patient)
        {
            try
            {
                AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

                var mappedPatient = _dto.MapFrontEndToPatientApi(patient);
                var json = JsonSerializer.Serialize(mappedPatient);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var SqlResponse = await _httpClient.PostAsync(_apiUrlSQL, content);

                if (SqlResponse.IsSuccessStatusCode)
                {
                    // Stocker un message de succès dans ViewData
                    _logger.LogTrace("Patient created successfully");

                }
                else
                {
                    _logger.LogError("Unable to create sqlpatient. Please try again.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating sqlpatient.");
            }

        }

        //Méthode Create pour poster un note au patient
        public async Task PostNoteCreate(PatientNote patientNote)
        {
            AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

            //DONE Mettre en relation Id patient et Id Note
            var sqlResponse = await _httpClient.GetAsync($"{_apiUrlSQL}/search?firstName={patientNote.Patient.FirstName}&lastName={patientNote.Patient.LastName}&dateOfBirth={patientNote.Patient.DateOfBirth.Year}-{patientNote.Patient.DateOfBirth.Month}-{patientNote.Patient.DateOfBirth.Day}");

            var json = await sqlResponse.Content.ReadAsStringAsync();
            var sqlPatient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var mongoPatient = new Note
            {
                PatId = sqlPatient.Id,
                Patient = patientNote.Patient.FirstName,
                NoteText = patientNote.NoteText.Note
            };
            var mongo = JsonSerializer.Serialize(mongoPatient);
            var mongocontent = new StringContent(mongo, Encoding.UTF8, "application/json");

            var mongoResponse = await _httpClient.PostAsync(_apiUrlMongo, mongocontent);

            if (mongoResponse.IsSuccessStatusCode)
            {
                _logger.LogTrace("Note created successfully");
            }
            else
            {
                _logger.LogError("Unable to create Note. Please try again.");
            }
        }

        //Methode pour récupérer la liste des données SQL de tous les patients dans la base
        public async Task<List<PatientNoteViewModel>> GetPatientManagement()
        {
            try
            {
                AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes
                var patientNoteViewModels = new List<PatientNoteViewModel>();
                var SqlResponse = await _httpClient.GetAsync(_apiUrlSQL);

                if (SqlResponse.IsSuccessStatusCode)
                {
                    var json = await SqlResponse.Content.ReadAsStringAsync();
                    var sqlPatients = JsonSerializer.Deserialize<List<Patient>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    foreach (Patient patient in sqlPatients)
                    {
                        var patientNoteViewModel = new PatientNoteViewModel();
                        patientNoteViewModel.Patient = _dto.MaPatientApiToPatientViewModel(patient);
                        patientNoteViewModel.RiskDiabete = await CalculateAssessmentDiabetePatient(patient.Id);
                        patientNoteViewModels.Add(patientNoteViewModel);
                    }
                    return patientNoteViewModels; // Assurez-vous que le nom de la vue est correct
                }
                else
                {
                    _logger.LogError("Unable to retrieve sqlPatients from API.");
                    return new List<PatientNoteViewModel>(); // Retourne une vue avec une liste vide
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sqlPatients.");
                return new List<PatientNoteViewModel>(); // Retourne une vue avec une liste vide
            }

        }

        public async Task<Patient> GetPatientSqlDataAsync(int patientId)
        {
            AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

            Patient patient = new Patient();

            var sqlResponse = await _httpClient.GetAsync($"{_apiUrlSQL}/{patientId}");
            if (sqlResponse.IsSuccessStatusCode)
            {
                var json = await sqlResponse.Content.ReadAsStringAsync();
                patient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            else
            {
                _logger.LogError($"Failed to fetch patient from SQL DB. Status Code: {sqlResponse.StatusCode}");
            }

            return patient;
        }

        public async Task<List<Note>> GetPatientMongoDataAsync(int patientId)
        {
            AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

            List<Note> noteList = new List<Note>();

            var mongoResponse = await _httpClient.GetAsync($"{_apiUrlMongo}/bypatid/{patientId}");
            if (mongoResponse.IsSuccessStatusCode)
            {
                var mongoJson = await mongoResponse.Content.ReadAsStringAsync();
                noteList = JsonSerializer.Deserialize<List<Note>>(mongoJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            }
            else
            {
                _logger.LogError($"Failed to fetch patient from Mongo DB.");
            }

            return noteList;
        }

        public async Task<PatientNoteViewModel> GetPatientDataAsync(int patientId)
        {
            PatientNoteViewModel patientNoteViewModel = new PatientNoteViewModel();

            try
            {
                AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

                // Récupérer les données SQL
                Patient sqlResponse = await GetPatientSqlDataAsync(patientId);

                if (sqlResponse != null)
                {

                    patientNoteViewModel.Patient = _dto.MaPatientApiToPatientViewModel(sqlResponse);

                }
                else
                {
                    _logger.LogError($"Failed to fetch patient from SQL DB.");
                }

                // Récupérer les données MongoDB (notes)
                List<Note> mongoResponse = await GetPatientMongoDataAsync(patientId);

                if (mongoResponse != null)
                {
                    // Ajouter les notes au modèle
                    foreach (Note mongoNote in mongoResponse)
                    {
                        NoteViewModel noteViewModel = _dto.MapNoteApiToFrontEnd(mongoNote);

                        patientNoteViewModel.Notes.Add(noteViewModel); // Ajouter chaque note à la liste
                    }
                }
                else
                {
                    _logger.LogError($"Failed to fetch patient from Mongo DB.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching patient data.");
            }

            return patientNoteViewModel;
        }

        public async Task UpdatePatientData(PatientViewModel updatedPatientViewModel)
        {
            AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes
            Patient mapPatientContent = _dto.MapFrontEndToPatientApi(updatedPatientViewModel);
            // Mise à jour du patient SQL
            var sqlPatientContent = new StringContent(JsonSerializer.Serialize(mapPatientContent), Encoding.UTF8, "application/json");
            var sqlResponse = await _httpClient.PutAsync($"{_apiUrlSQL}/{mapPatientContent.Id}", sqlPatientContent);

            if (!sqlResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to update SQL Patient. Status Code: {sqlResponse.StatusCode}");

            }
        }

        public async Task UpdateNoteData(NoteViewModel updatedNoteViewModel)
        {
            AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

            Note mapNoteContent = _dto.MapFrontEndToNoteApi(updatedNoteViewModel);
            _logger.LogInformation("updatedNoteViewModel :" + updatedNoteViewModel.ToString());
            _logger.LogInformation("mapnotecontent : " + mapNoteContent.ToString());
            var mongoNoteContent = new StringContent(JsonSerializer.Serialize(mapNoteContent), Encoding.UTF8, "application/json");
            var mongoResponse = await _httpClient.PutAsync($"{_apiUrlMongo}/byid/{updatedNoteViewModel.Id}", mongoNoteContent);
            if (!mongoResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to update SQL Patient. Status Code: {mongoResponse.StatusCode}");

            }
        }

        public async Task DeletePatientAndNote(int patientId)
        {
            try
            {
                AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

                var SqlResponse = await _httpClient.DeleteAsync($"{_apiUrlSQL}/{patientId}");

                var MongoResponse = await _httpClient.DeleteAsync($"{_apiUrlMongo}/bypatid/{patientId}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting datas Patient.");
            }
        }

        //Calcule le risque de diabete
        public async Task<string> CalculateAssessmentDiabetePatient(int patientId)
        {
            string assessmentRisk = string.Empty; // Initialiser la chaîne de caractères

            try
            {
                AddJwtTokenToRequestHeaders(); // Ajouter le token dans les en-têtes

                // Effectuer la requête HTTP pour obtenir l'évaluation
                var request = await _httpClient.GetAsync($"{_apiUrlReport}/{patientId}");

                // Vérifier si la requête a réussi
                if (request.IsSuccessStatusCode)
                {
                    // Lire le contenu de la réponse qui est en type string et non JSON
                    assessmentRisk = await request.Content.ReadAsStringAsync();

                }
                else
                {
                    // Ajouter des informations supplémentaires dans les logs
                    _logger.LogError($"Failed to fetch patient assessment. Status code: {request.StatusCode}. Reason: {await request.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                // Loguer l'exception en cas d'erreur
                _logger.LogError(ex, "An error occurred while fetching the patient assessment.");
            }

            return assessmentRisk;
        }
    }
}

