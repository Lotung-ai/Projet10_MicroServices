using MicroServiceReport.Models;
using Microsoft.EntityFrameworkCore;


namespace MicroServiceReport.Data
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Empêche EF de tenter de créer ou d'ajuster le schéma
            modelBuilder.Entity<Patient>().ToTable("Patients");
        }
    }
}
