using MicroFrontEnd.Models;
using MicroFrontEnd.Services.Interfaces;
using MicroServicePatient.Models;
using MicroServiceNote.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MicroFrontEnd.Services
{
    public class FrontService : IFrontService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FrontService> _logger;
        private readonly string _apiUrlSQL = "http://ocelotapigw:80/gateway/patients";
        private readonly string _apiUrlMongo = "http://ocelotapigw:80/gateway/notemongo";
        private readonly string _apiUrlReport = "http://ocelotapigw:80/gateway/report";


        public FrontService(IHttpClientFactory httpClientFactory, ILogger<FrontService> logger)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public Patient MapFrontEndToPatientApi(PatientViewModel patientViewModel)
        {
            var patient = new Patient
            {
                Id = patientViewModel.Id,
                FirstName = patientViewModel.FirstName,
                LastName = patientViewModel.LastName,
                DateOfBirth = patientViewModel.DateOfBirth,
                Gender = patientViewModel.Gender,
                Address = patientViewModel.Address,
                PhoneNumber = patientViewModel.PhoneNumber
            };

            return patient;
        }

        public PatientViewModel MaPatientApiToPatientViewModel(Patient patient)
        {
            var patientViewModel = new PatientViewModel();

            patientViewModel.Id = patient.Id;
            patientViewModel.FirstName = patient.FirstName;
            patientViewModel.LastName = patient.LastName;
            patientViewModel.DateOfBirth = patient.DateOfBirth;
            patientViewModel.Gender = patient.Gender;
            patientViewModel.Address = patient.Address;
            patientViewModel.PhoneNumber = patient.PhoneNumber;

            return patientViewModel;
        }

        public Note MapFrontEndToNoteApi(NoteViewModel noteViewModel)
        {
            var note = new Note();

            note.Id = noteViewModel.Id;
            note.PatId = noteViewModel.PatId;
            note.Patient = noteViewModel.Patient;
            note.NoteText = noteViewModel.Note;

            return note;
        }

        public NoteViewModel MapNoteApiToFrontEnd(Note note)
        {
            var noteViewModel = new NoteViewModel
            {
                Id = note.Id,
                PatId = note.PatId,
                Patient = note.Patient,
                Note = note.NoteText

            };

            return noteViewModel;
        }

        //Méthode Create pour créer un sqlPatient
        public async Task PostPatientCreate(PatientViewModel patient)
        {
            try
            {
                var mappedPatient = MapFrontEndToPatientApi(patient);
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

        public async Task<Patient> GetPatientSqlDataAsync(int patientId)
        {
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
                // Récupérer les données SQL
                Patient sqlResponse = await GetPatientSqlDataAsync(patientId);

                if (sqlResponse != null)
                {

                    patientNoteViewModel.Patient = MaPatientApiToPatientViewModel(sqlResponse);

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
                        NoteViewModel noteViewModel = MapNoteApiToFrontEnd(mongoNote);

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
            Patient mapPatientContent = MapFrontEndToPatientApi(updatedPatientViewModel);
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
            Note mapNoteContent = MapFrontEndToNoteApi(updatedNoteViewModel);
            _logger.LogInformation("updatedNoteViewModel :" + updatedNoteViewModel.ToString());
            _logger.LogInformation("mapnotecontent : " + mapNoteContent.ToString());
            var mongoNoteContent = new StringContent(JsonSerializer.Serialize(mapNoteContent), Encoding.UTF8, "application/json");
            var mongoResponse = await _httpClient.PutAsync($"{_apiUrlMongo}/byid/{updatedNoteViewModel.Id}", mongoNoteContent);
            if (!mongoResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to update SQL Patient. Status Code: {mongoResponse.StatusCode}");

            }
        }

        public async Task DeleteNoteDataWithPatientId(PatientViewModel patientViewModel)
        {
            var sqlResponse = await _httpClient.GetAsync($"{_apiUrlSQL}/search?firstName={patientViewModel.FirstName}&lastName={patientViewModel.LastName}&dateOfBirth={patientViewModel.DateOfBirth.Year}-{patientViewModel.DateOfBirth.Month}-{patientViewModel.DateOfBirth.Day}");

            var json = await sqlResponse.Content.ReadAsStringAsync();
            var sqlPatient = JsonSerializer.Deserialize<Patient>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        }

        //Calcule le risque de diabete
        public async Task<string> CalculateAssessmentDiabetePatient(int patientId)
        {
            string assessmentRisk = string.Empty; // Initialiser la chaîne de caractères

            try
            {
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

