using MicroServiceReport.Data;
using MicroServiceReport.Models;
using MicroServiceReport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MicroServiceReport.Services
{
    public class PatientService : IPatientService
    {
        private readonly SqlDbContext _context;

        public PatientService(SqlDbContext context)
        {
            _context = context;
        }

        /// Récupère une patient spécifique par son identifiant.
        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            // Rechercher l'offre par son identifiant.
            return await _context.Patients.FindAsync(id);
        }

        /// Récupère une patient avec son nom, prénom et date de naissance.
        public async Task<Patient> GetPatientByNameDob(string firstName, string lastName, DateTime dateOfBirth)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.FirstName == firstName
                                          && p.LastName == lastName
                                          && p.DateOfBirth == dateOfBirth);

            return patient;
        }

        /// Récupère toutes les offres disponibles.
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            // Récupérer toutes les offres sous forme de liste.
            return await _context.Patients.ToListAsync();
        }

    }
}