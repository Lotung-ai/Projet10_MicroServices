using MicroServices.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace MicroServices.Data
{
    public class PatientDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public PatientDbContext(DbContextOptions<PatientDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
            .HasKey(cp => cp.Id);

            // Additional configuration for Identity
            builder.Entity<IdentityUserLogin<int>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            builder.Entity<IdentityUserRole<int>>().HasKey(r => new { r.UserId, r.RoleId });
            builder.Entity<IdentityUserToken<int>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
        }
    }
}
