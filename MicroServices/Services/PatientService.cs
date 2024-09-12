using MicroServicePatient.Data;
using MicroServicePatient.Models;
using MicroServicePatient.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MicroServicePatient.Services
{
    public class PatientService : IPatientService
    {
        private readonly PatientDbContext _context;

        public PatientService(PatientDbContext context)
        {
            _context = context;
        }

        /// Crée une nouvelle offre en l'ajoutant à la base de données.
        public async Task<Patient> CreatePatientAsync(Patient Patient)
        {
            // Ajouter l'offre à la collection Patients du contexte.
            _context.Patients.Add(Patient);
            // Sauvegarder les modifications dans la base de données.
            await _context.SaveChangesAsync();
            // Retourner l'offre ajoutée.
            return Patient;
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

        /// Met à jour une offre existante dans la base de données.
        public async Task<Patient> UpdatePatientAsync(Patient Patient)
        {
            // Modifier l'état de l'entité pour indiquer qu'elle est en mode de mise à jour.
            _context.Patients.Update(Patient);
            // Sauvegarder les modifications dans la base de données.
            await _context.SaveChangesAsync();
            // Retourner l'offre mise à jour.
            return Patient;
        }

        /// Supprime une offre de la base de données par son identifiant.
        public async Task<bool> DeletePatientAsync(int id)
        {
            // Rechercher l'offre à supprimer.
            var Patient = await _context.Patients.FindAsync(id);
            // Vérifier si l'offre existe.
            if (Patient == null) return false;

            // Supprimer l'offre de la collection Patients du contexte.
            _context.Patients.Remove(Patient);
            // Sauvegarder les modifications dans la base de données.
            await _context.SaveChangesAsync();
            // Retourner vrai pour indiquer que la suppression a réussi.
            return true;
        }
    }
}