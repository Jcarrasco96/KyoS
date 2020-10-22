using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Data
{
    public class DataContext : IdentityDbContext<UserEntity>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        
        public DbSet<ClinicEntity> Clinics { get; set; }
        public DbSet<ClientEntity> Clients { get; set; }
        public DbSet<SupervisorEntity> Supervisors { get; set; }
        public DbSet<FacilitatorEntity> Facilitators { get; set; }
        public DbSet<ThemeEntity> Themes { get; set; }
        public DbSet<ActivityEntity> Activities { get; set; }
        public DbSet<NoteEntity> Notes { get; set; }
        public DbSet<Clinic_Theme> Clinics_Themes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClinicEntity>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<FacilitatorEntity>()
                .HasIndex(s => s.Name)
                .IsUnique();

            modelBuilder.Entity<SupervisorEntity>()
                .HasIndex(s => s.Name)
                .IsUnique();            
        }
    }
}
