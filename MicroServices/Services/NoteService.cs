using MicroServices.Data;
using MicroServices.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MicroServices.Services
{
    public class NoteService
    {
        private readonly IMongoCollection<Note> _patientsCollection;

        public NoteService(IOptions<PatientDbSettings> patientDatabaseSettings)
        {
            var mongoClient = new MongoClient(patientDatabaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(patientDatabaseSettings.Value.DatabaseName);
            _patientsCollection = mongoDatabase.GetCollection<Note>(patientDatabaseSettings.Value.PatientsCollectionName);
        }

        public async Task<List<Note>> GetAsync() =>
            await _patientsCollection.Find(_ => true).ToListAsync();

        public async Task<Note?> GetAsync(string id) =>
            await _patientsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        public async Task<List<Note>> GetPatientByPatIdAsync(int patId) =>
            await _patientsCollection.Find(Builders<Note>.Filter.Eq(p => p.PatId, patId)).ToListAsync();
        
        public async Task CreateAsync(Note newPatient) =>
            await _patientsCollection.InsertOneAsync(newPatient);

        public async Task UpdateAsync(string id, Note updatedPatient) =>
            await _patientsCollection.ReplaceOneAsync(x => x.Id == id, updatedPatient);

        public async Task RemoveAsync(string id) =>
            await _patientsCollection.DeleteOneAsync(x => x.Id == id);
        public async Task DeletePatientByPatIdAsync(int patId) =>
            await _patientsCollection.DeleteManyAsync(Builders<Note>.Filter.Eq(p => p.PatId, patId));
    }
}
