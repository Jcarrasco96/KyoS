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
        public DbSet<DiagnosticTempEntity> DiagnosticsTemp { get; set; }
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
        public DbSet<ReferredEntity> Referreds { get; set; }
        public DbSet<LegalGuardianEntity> LegalGuardians { get; set; }
        public DbSet<EmergencyContactEntity> EmergencyContacts { get; set; }
        public DbSet<DoctorEntity> Doctors { get; set; }
        public DbSet<PsychiatristEntity> Psychiatrists { get; set; }
        public DbSet<DiagnosticEntity> Diagnostics { get; set; }
        public DbSet<Client_Diagnostic> Clients_Diagnostics { get; set; }
        public DbSet<DocumentDiagnosticEntity> DocumentDiagnostics { get; set; }
        public DbSet<DocumentEntity> Documents { get; set; }
        public DbSet<DocumentTempEntity> DocumentsTemp { get; set; }
        public DbSet<TemplateDOCEntity> TemplatesDOC { get; set; }
        public DbSet<IncidentEntity> Incidents { get; set; }
        public DbSet<HealthInsuranceEntity> HealthInsurances { get; set; }
        public DbSet<Client_HealthInsurance> Clients_HealthInsurances { get; set; }
        public DbSet<IndividualNoteEntity> IndividualNotes { get; set; }
        public DbSet<GroupNoteEntity> GroupNotes { get; set; }
        public DbSet<GroupNote_Activity> GroupNotes_Activities { get; set; }
        public DbSet<SettingEntity> Settings { get; set; }

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

            //modelBuilder.Entity<ClientEntity>()
            //    .HasIndex(c => c.Name)
            //    .IsUnique();

            modelBuilder.Entity<GoalEntity>()
                        .HasOne(g => g.MTP)
                        .WithMany(m => m.Goals)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ObjetiveEntity>()
                        .HasOne(o => o.Goal)
                        .WithMany(g => g.Objetives)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Objetive_Classification>()
                        .HasOne(oc => oc.Objetive)
                        .WithMany(o => o.Classifications)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NotePrototype_Classification>()
                        .HasOne(nc => nc.Note)
                        .WithMany(n => n.Classifications)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Plan_Classification>()
                        .HasOne(nc => nc.Plan)
                        .WithMany(n => n.Classifications)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentEntity>()
                        .HasOne(d => d.Client)
                        .WithMany(c => c.Documents)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client_Diagnostic>()
                        .HasOne(cd => cd.Client)
                        .WithMany(c => c.Clients_Diagnostics)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.Note)
                        .WithOne(n => n.Workday_Cient)
                        .HasForeignKey<NoteEntity>(n => n.Workday_Client_FK);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.Note)
                        .WithOne(n => n.Workday_Cient)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.IndividualNote)
                        .WithOne(n => n.Workday_Cient)
                        .HasForeignKey<IndividualNoteEntity>(i => i.Workday_Client_FK);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.IndividualNote)
                        .WithOne(n => n.Workday_Cient)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.GroupNote)
                        .WithOne(gn => gn.Workday_Cient)
                        .HasForeignKey<GroupNoteEntity>(g => g.Workday_Client_FK);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.GroupNote)
                        .WithOne(n => n.Workday_Cient)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClinicEntity>()
                        .HasOne(c => c.Setting)
                        .WithOne(s => s.Clinic)
                        .HasForeignKey<SettingEntity>(s => s.Clinic_FK);

            modelBuilder.Entity<ClinicEntity>()
                        .HasOne(c => c.Setting)
                        .WithOne(s => s.Clinic)
                        .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
