using MicroServices.Data;
using MicroServices.Models;
using MicroServices.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MicroServices.Services
{
    /// <summary>
    /// Implémentation du dépôt pour les opérations CRUD sur l'entité <see cref="Patient"/>.
    /// Cette classe implémente l'interface <see cref="IPatientRepository"/> et utilise Entity Framework Core pour interagir avec la base de données.
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly PatientDbContext _context;

        /// <summary>
        /// Constructeur pour injecter le contexte de base de données dans le dépôt.
        /// </summary>
        /// <param name="context">Contexte de la base de données utilisé pour les opérations CRUD.</param>
        public PatientService(PatientDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crée une nouvelle offre en l'ajoutant à la base de données.
        /// </summary>
        /// <param name="Patient">L'objet <see cref="Patient"/> représentant l'offre à ajouter.</param>
        /// <returns>La tâche représentant l'opération asynchrone, avec l'offre ajoutée comme résultat.</returns>
        public async Task<Patient> CreatePatientAsync(Patient Patient)
        {
            // Ajouter l'offre à la collection Patients du contexte.
            _context.Patients.Add(Patient);
            // Sauvegarder les modifications dans la base de données.
            await _context.SaveChangesAsync();
            // Retourner l'offre ajoutée.
            return Patient;
        }

        /// <summary>
        /// Récupère une offre spécifique par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'offre à récupérer.</param>
        /// <returns>La tâche représentant l'opération asynchrone, avec l'offre trouvée comme résultat.</returns>
        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            // Rechercher l'offre par son identifiant.
            return await _context.Patients.FindAsync(id);
        }

        /// <summary>
        /// Récupère toutes les offres disponibles.
        /// </summary>
        /// <returns>La tâche représentant l'opération asynchrone, avec une collection des offres comme résultat.</returns>
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            // Récupérer toutes les offres sous forme de liste.
            return await _context.Patients.ToListAsync();
        }

        /// <summary>
        /// Met à jour une offre existante dans la base de données.
        /// </summary>
        /// <param name="Patient">L'objet <see cref="Patient"/> contenant les données mises à jour de l'offre.</param>
        /// <returns>La tâche représentant l'opération asynchrone, avec l'offre mise à jour comme résultat.</returns>
        public async Task<Patient> UpdatePatientAsync(Patient Patient)
        {
            // Modifier l'état de l'entité pour indiquer qu'elle est en mode de mise à jour.
            _context.Patients.Update(Patient);
            // Sauvegarder les modifications dans la base de données.
            await _context.SaveChangesAsync();
            // Retourner l'offre mise à jour.
            return Patient;
        }

        /// <summary>
        /// Supprime une offre de la base de données par son identifiant.
        /// </summary>
        /// <param name="id">L'identifiant de l'offre à supprimer.</param>
        /// <returns>La tâche représentant l'opération asynchrone, avec un booléen indiquant si la suppression a réussi.</returns>
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