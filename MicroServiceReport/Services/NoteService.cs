using MicroServiceReport.Data;
using MicroServiceReport.Models;
using MicroServiceReport.Services.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MicroServiceReport.Services
{
    public class NoteService : INoteService
    {
        private readonly IMongoCollection<Note> _patientsCollection;

        public NoteService(IOptions<MongoDbSettings> patientDatabaseSettings)
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

    }
}
