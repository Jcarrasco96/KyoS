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
        public DbSet<NotePrototypeEntity> NotesPrototypes { get; set; }
        public DbSet<NotePrototype_Classification> NotesPrototypes_Classifications { get; set; }        
        public DbSet<DiagnosisEntity> Diagnoses { get; set; }
        public DbSet<DiagnosisTempEntity> DiagnosesTemp { get; set; }
        public DbSet<MTPEntity> MTPs { get; set; }
        public DbSet<GoalEntity> Goals { get; set; }
        public DbSet<ObjetiveEntity> Objetives { get; set; }
        public DbSet<ClassificationEntity> Classifications { get; set; }
        public DbSet<Objetive_Classification> Objetives_Classifications { get; set; }       
        public DbSet<GroupEntity> Groups { get; set; }
        public DbSet<DailySessionEntity> DailySessions  { get; set; }
        public DbSet<GeneratedNoteEntity> GeneratedNotes { get; set; }
        public DbSet<GeneratedNote_NotePrototype> GeneratedNotes_NotesPrototypes { get; set; }
        public DbSet<PlanEntity> Plans { get; set; }
        public DbSet<Plan_Classification> Plans_Classifications { get; set; }
        public DbSet<WeekEntity> Weeks { get; set; }
        public DbSet<WorkdayEntity> Workdays { get; set; }
        public DbSet<Workday_Client> Workdays_Clients { get; set; }
        public DbSet<NoteEntity> Notes { get; set; }
        public DbSet<Note_Activity> Notes_Activities { get; set; }
        public DbSet<MessageEntity> Messages { get; set; }
        public DbSet<Workday_Activity_Facilitator> Workdays_Activities_Facilitators { get; set; }

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
            modelBuilder.Entity<NotePrototype_Classification>()
                .HasOne(nc => nc.Note).WithMany(n => n.Classifications).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Plan_Classification>()
                .HasOne(nc => nc.Plan).WithMany(n => n.Classifications).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workday_Client>()
           .HasOne(wd => wd.Note)
           .WithOne(n => n.Workday_Cient)
           .HasForeignKey<NoteEntity>(n => n.Workday_Client_FK);

            modelBuilder.Entity<Workday_Client>()
                .HasOne(wd => wd.Note).WithOne(n => n.Workday_Cient).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
