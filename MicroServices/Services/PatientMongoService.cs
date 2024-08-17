using MicroServices.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MicroServices.Services
{
    public class PatientMongoService
    {
        private readonly IMongoCollection<PatientMongo> _patientsCollection;

        public PatientMongoService(IOptions<PatientDbSettings> patientDatabaseSettings)
        {
            var mongoClient = new MongoClient(patientDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(patientDatabaseSettings.Value.DatabaseName);
            _patientsCollection = mongoDatabase.GetCollection<PatientMongo>(patientDatabaseSettings.Value.PatientsCollectionName);
        }

        public async Task<List<PatientMongo>> GetAsync() =>
            await _patientsCollection.Find(_ => true).ToListAsync();

        public async Task<PatientMongo?> GetAsync(string id) =>
            await _patientsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(PatientMongo newPatient) =>
            await _patientsCollection.InsertOneAsync(newPatient);

        public async Task UpdateAsync(string id, PatientMongo updatedPatient) =>
            await _patientsCollection.ReplaceOneAsync(x => x.Id == id, updatedPatient);

        public async Task RemoveAsync(string id) =>
            await _patientsCollection.DeleteOneAsync(x => x.Id == id);
    }
}
