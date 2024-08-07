using MicroServices.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MicroServices.Data
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
