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
        public DbSet<Note_Classification> Notes_Classifications { get; set; }
        public DbSet<Clinic_Theme> Clinics_Themes { get; set; }
        public DbSet<DiagnosisEntity> Diagnoses { get; set; }
        public DbSet<DiagnosisTempEntity> DiagnosesTemp { get; set; }
        public DbSet<MTPEntity> MTPs { get; set; }
        public DbSet<GoalEntity> Goals { get; set; }
        public DbSet<ObjetiveEntity> Objetives { get; set; }
        public DbSet<ClassificationEntity> Classifications { get; set; }
        public DbSet<Objetive_Classification> Objetives_Classifications { get; set; }       
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<DailySessionEntity> DailySessions  { get; set; }
        public DbSet<Note_DailySession_Client> Notes_DailySessions_Clients { get; set; }
        public DbSet<PlanEntity> Plans { get; set; }
        public DbSet<Plan_Classification> Plans_Classifications { get; set; }

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

            modelBuilder.Entity<DiagnosisEntity>()
                .HasOne(d => d.MTP).WithMany(m => m.Diagnosis).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<GoalEntity>()
                .HasOne(g => g.MTP).WithMany(m => m.Goals).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ObjetiveEntity>()
                .HasOne(o => o.Goal).WithMany(g => g.Objetives).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Objetive_Classification>()
                .HasOne(oc => oc.Objetive).WithMany(o => o.Classifications).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Note_Classification>()
                .HasOne(nc => nc.Note).WithMany(n => n.Classifications).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Plan_Classification>()
                .HasOne(nc => nc.Plan).WithMany(n => n.Classifications).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
