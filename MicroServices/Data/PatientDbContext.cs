using MicroServicePatient.Models;
using Microsoft.EntityFrameworkCore;


namespace MicroServicePatient.Data
{
    public class PatientDbContext : DbContext
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }

    }
}
