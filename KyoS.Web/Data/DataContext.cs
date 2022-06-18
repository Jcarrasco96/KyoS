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
        public DbSet<TCMSupervisorEntity> TCMSupervisors { get; set; }
        public DbSet<FacilitatorEntity> Facilitators { get; set; }
        public DbSet<CaseMannagerEntity> CaseManagers { get; set; }
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
        public DbSet<NotePEntity> NotesP { get; set; }
        public DbSet<NoteP_Activity> NotesP_Activities { get; set; }
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
        public DbSet<TCMServiceEntity> TCMServices { get; set; }
        public DbSet<TCMStageEntity> TCMStages { get; set; }
        public DbSet<TCMClientEntity> TCMClient { get; set; }
        public DbSet<TCMObjetiveEntity> TCMObjetives { get; set; }
        public DbSet<TCMDomainEntity> TCMDomains { get; set; }
        public DbSet<TCMServicePlanEntity> TCMServicePlans { get; set; }
        public DbSet<TCMAdendumEntity> TCMAdendums { get; set; }
        public DbSet<TCMServicePlanReviewDomainEntity> TCMServicePlanReviewDomains { get; set; }
        public DbSet<TCMServicePlanReviewEntity> TCMServicePlanReviews { get; set; }
        public DbSet<TCMServicePlanReviewDomainObjectiveEntity> TCMServicePlanReviewDomainObjectives { get; set; }
        public DbSet<TCMDischargeFollowUpEntity> TCMDischargeFollowUp { get; set; }
        public DbSet<TCMDischargeEntity> TCMDischarge { get; set; }
        public DbSet<TCMDischargeServiceStatusEntity> TCMDischargeServiceStatus { get; set; }
        public DbSet<IntakeScreeningEntity> IntakeScreenings { get; set; }
        public DbSet<IntakeConsentForTreatmentEntity> IntakeConsentForTreatment { get; set; }
        public DbSet<IntakeConsentForReleaseEntity> IntakeConsentForRelease { get; set; }
        public DbSet<IntakeConsumerRightsEntity> IntakeConsumerRights { get; set; }
        public DbSet<IntakeAcknowledgementHippaEntity> IntakeAcknowledgement { get; set; }
        public DbSet<IntakeAccessToServicesEntity> IntakeAccessToServices { get; set; }
        public DbSet<IntakeOrientationChecklistEntity> IntakeOrientationCheckList { get; set; }
        public DbSet<IntakeTransportationEntity> IntakeTransportation { get; set; }
        public DbSet<IntakeConsentPhotographEntity> IntakeConsentPhotograph { get; set; }
        public DbSet<IntakeFeeAgreementEntity> IntakeFeeAgreement { get; set; }
        public DbSet<IntakeTuberculosisEntity> IntakeTuberculosis { get; set; }
        public DbSet<IntakeMedicalHistoryEntity> IntakeMedicalHistory { get; set; }
        public DbSet<DischargeEntity> Discharge { get; set; }
        public DbSet<MedicationEntity> Medication { get; set; }
        public DbSet<FarsFormEntity> FarsForm { get; set; }
        public DbSet<BioEntity> Bio { get; set; }
        public DbSet<Bio_BehavioralHistoryEntity> Bio_BehavioralHistory { get; set; }
        public DbSet<AdendumEntity> Adendums { get; set; }
        public DbSet<MTPReviewEntity> MTPReviews { get; set; }
        public DbSet<TCMIntakeFormEntity> TCMIntakeForms { get; set; }
        public DbSet<TCMIntakeConsentForTreatmentEntity> TCMIntakeConsentForTreatment { get; set; }
        public DbSet<TCMIntakeConsentForReleaseEntity> TCMIntakeConsentForRelease { get; set; }
        public DbSet<TCMIntakeConsumerRightsEntity> TCMIntakeConsumerRights { get; set; }
        public DbSet<TCMIntakeAcknowledgementHippaEntity> TCMIntakeAcknowledgement { get; set; }
        public DbSet<TCMIntakeOrientationChecklistEntity> TCMIntakeOrientationCheckList { get; set; }
        public DbSet<TCMIntakeAdvancedDirectiveEntity> TCMIntakeAdvancedDirective { get; set; }
        public DbSet<TCMIntakeForeignLanguageEntity> TCMIntakeForeignLanguage { get; set; }
        public DbSet<TCMIntakeWelcomeEntity> TCMIntakeWelcome { get; set; }
        public DbSet<TCMIntakeNonClinicalLogEntity> TCMIntakeNonClinicalLog { get; set; }
        public DbSet<TCMIntakeMiniMentalEntity> TCMIntakeMiniMental { get; set; }
        public DbSet<TCMIntakeCoordinationCareEntity> TCMIntakeCoordinationCare { get; set; }
        public DbSet<TCMIntakeAppendixJEntity> TCMIntakeAppendixJ { get; set; }
        public DbSet<TCMIntakeInterventionLogEntity> TCMIntakeInterventionLog { get; set; }
        public DbSet<TCMIntakeInterventionEntity> TCMIntakeIntervention { get; set; }

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
            
            modelBuilder.Entity<TCMSupervisorEntity>()
                        .HasIndex(s => s.Name)
                        .IsUnique();

            modelBuilder.Entity<CaseMannagerEntity>()
                        .HasIndex(s => s.Name)
                        .IsUnique();

            modelBuilder.Entity<TCMServiceEntity>()
                        .HasIndex(s => s.Name)
                        .IsUnique();

            modelBuilder.Entity<TCMStageEntity>()
                        .HasOne(o => o.tCMservice)
                        .WithMany(g => g.Stages)
                        .OnDelete(DeleteBehavior.Cascade);

            /*modelBuilder.Entity<TCMStageEntity>()
                        .HasOne(o => o.tCMservice)
                        .WithMany(g => g.ID_Etapa)
                        .OnDelete(DeleteBehavior.Cascade);*/

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
                        .HasOne(wd => wd.NoteP)
                        .WithOne(n => n.Workday_Cient)
                        .HasForeignKey<NotePEntity>(n => n.Workday_Client_FK);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.NoteP)
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

            modelBuilder.Entity<TCMServicePlanEntity>()
                        .HasMany(c => c.TCMDomain)
                        .WithOne(s => s.TcmServicePlan)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMObjetiveEntity>()
                        .HasOne(o => o.TcmDomain)
                        .WithMany(g => g.TCMObjetive)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TcmServicePlan)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMServicePlanEntity>(s => s.TcmClient_FK);


            modelBuilder.Entity<TCMAdendumEntity>()
                                   .HasIndex(s => s.Id)
                                   .IsUnique();

            modelBuilder.Entity<TCMServicePlanReviewDomainEntity>()
                                   .HasIndex(s => s.Id)
                                   .IsUnique();

            modelBuilder.Entity<TCMServicePlanEntity>()
                        .HasOne(c => c.TCMServicePlanReview)
                        .WithOne(s => s.TcmServicePlan)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMServicePlanReviewEntity>(s => s.TcmServicePlan_FK);

            modelBuilder.Entity<TCMServicePlanReviewDomainObjectiveEntity>()
                                   .HasIndex(s => s.Id)
                                   .IsUnique();

            modelBuilder.Entity<TCMServicePlanEntity>()
                        .HasOne(c => c.TCMDischarge)
                        .WithOne(s => s.TcmServicePlan)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMDischargeEntity>(s => s.TcmServicePlan_FK);

            modelBuilder.Entity<TCMDischargeFollowUpEntity>()
                       .HasOne(o => o.TcmDischarge)
                       .WithMany(g => g.TcmDischargeFollowUp)
                       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMDischargeServiceStatusEntity>()
                       .HasOne(o => o.TcmDischarge)
                       .WithMany(g => g.TcmDischargeServiceStatus)
                       .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeScreening)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeScreeningEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeConsentForTreatment)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeConsentForTreatmentEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeConsentForRelease)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeConsentForReleaseEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeConsumerRights)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeConsumerRightsEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                       .HasOne(c => c.IntakeAcknowledgementHipa)
                       .WithOne(s => s.Client)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<IntakeAcknowledgementHippaEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeAccessToServices)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeAccessToServicesEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeOrientationChecklist)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeOrientationChecklistEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeTransportation)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeTransportationEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeConsentPhotograph)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeConsentPhotographEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeFeeAgreement)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeFeeAgreementEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeTuberculosis)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeTuberculosisEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.IntakeMedicalHistory)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<IntakeMedicalHistoryEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                        .HasMany(c => c.DischargeList)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade);
                       
            modelBuilder.Entity<ClientEntity>()
                        .HasMany(c => c.MedicationList)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.Bio)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<BioEntity>(s => s.Client_FK);

            modelBuilder.Entity<Bio_BehavioralHistoryEntity>()
                        .HasOne(o => o.Client)
                        .WithMany(g => g.List_BehavioralHistory)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MTPEntity>()
                        .HasMany(c => c.AdendumList)
                        .WithOne(s => s.Mtp)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MTPEntity>()
                       .HasMany(c => c.MtpReviewList)
                       .WithOne(s => s.Mtp)
                       .OnDelete(DeleteBehavior.Cascade)
                       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeForm)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeFormEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TcmIntakeConsentForTreatment)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeConsentForTreatmentEntity>(s => s.Client_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasMany(c => c.TcmIntakeConsentForRelease)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade);
                        

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TcmIntakeConsumerRights)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeConsumerRightsEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TcmIntakeAcknowledgementHipa)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeAcknowledgementHippaEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeOrientationChecklist)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeOrientationChecklistEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeAdvancedDirective)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeAdvancedDirectiveEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeForeignLanguage)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeForeignLanguageEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeWelcome)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeWelcomeEntity>(s => s.TcmClient_FK);
 
            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeNonClinicalLog)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeNonClinicalLogEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeMiniMental)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeMiniMentalEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeCoordinationCare)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeCoordinationCareEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                       .HasOne(c => c.TcmIntakeAppendixJ)
                       .WithOne(s => s.TcmClient)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<TCMIntakeAppendixJEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                       .HasOne(c => c.TcmInterventionLog)
                       .WithOne(s => s.TcmClient)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<TCMIntakeInterventionLogEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMIntakeInterventionEntity>()
                       .HasOne(g => g.TcmInterventionLog)
                       .WithMany(m => m.InterventionList)
                       .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
