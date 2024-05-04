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
        public DbSet<Client_Referred> Clients_Referreds { get; set; }
        public DbSet<ReferredTempEntity> ReferredsTemp { get; set; }
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
        public DbSet<BioTempEntity> BioTemp { get; set; }
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
        public DbSet<TCMFarsFormEntity> TCMFarsForm { get; set; }
        public DbSet<TCMAssessmentEntity> TCMAssessment { get; set; }
        public DbSet<TCMAssessmentTempEntity> TCMAssessmentTemp { get; set; }
        public DbSet<TCMAssessmentIndividualAgencyEntity> TCMAssessmentIndividualAgency { get; set; }
        public DbSet<TCMAssessmentHouseCompositionEntity> TCMAssessmentHouseComposition { get; set; }
        public DbSet<TCMAssessmentPastCurrentServiceEntity> TCMAssessmentPastCurrentService { get; set; }
        public DbSet<TCMAssessmentMedicationEntity> TCMAssessmentMedication { get; set; }
        public DbSet<TCMAssessmentHospitalEntity> TCMAssessmentHospital { get; set; }
        public DbSet<TCMAssessmentDrugEntity> TCMAssessmentDrug { get; set; }
        public DbSet<TCMAssessmentMedicalProblemEntity> TCMAssessmentMedicalProblem { get; set; }
        public DbSet<TCMAssessmentSurgeryEntity> TCMAssessmentSurgery { get; set; }
        public DbSet<TCMNoteEntity> TCMNote { get; set; }
        public DbSet<TCMNoteActivityEntity> TCMNoteActivity { get; set; }
        public DbSet<DocumentsAssistantEntity> DocumentsAssistant { get; set; }
        public DbSet<TCMServiceActivityEntity> TCMServiceActivity { get; set; }
        public DbSet<TCMNoteActivityTempEntity> TCMNoteActivityTemp { get; set; }
        public DbSet<TCMMessageEntity> TCMMessages { get; set; }
        public DbSet<GoalsTempEntity> GoalsTemp { get; set; }
        public DbSet<ObjectiveTempEntity> ObjetivesTemp { get; set; }
        public DbSet<HealthInsuranceTempEntity> HealthInsuranceTemp { get; set; }
        public DbSet<BriefEntity> Brief { get; set; }
        public DbSet<GroupNote2Entity> GroupNotes2 { get; set; }
        public DbSet<GroupNote2_Activity> GroupNotes2_Activities { get; set; }
        public DbSet<ScheduleEntity> Schedule { get; set; }
        public DbSet<SubScheduleEntity> SubSchedule { get; set; }
        public DbSet<ManagerEntity> Manager { get; set; }
        public DbSet<EligibilityEntity> Eligibilities { get; set; }
        public DbSet<TCMIntakeClientSignatureVerificationEntity> TCMIntakeClientSignatureVerification { get; set; }
        public DbSet<TCMIntakeClientIdDocumentVerificationEntity> TCMIntakeClientDocumentVerification { get; set; }
        public DbSet<TCMIntakePainScreenEntity> TCMIntakePainScreen { get; set; }
        public DbSet<TCMIntakeColumbiaSuicideEntity> TCMIntakeColumbiaSuicide { get; set; }
        public DbSet<TCMIntakeNutritionalScreenEntity> TCMIntakeNutritionalScreen { get; set; }
        public DbSet<TCMIntakePersonalWellbeingEntity> TCMIntakePersonalWellbeing { get; set; }
        public DbSet<TCMDateBlockedEntity> TCMDateBlocked { get; set; }
        public DbSet<CiteEntity> Cites { get; set; }
        public DbSet<BillDmsEntity> BillDms { get; set; }
        public DbSet<BillDmsDetailsEntity> BillDmsDetails { get; set; }
        public DbSet<BillDmsPaidEntity> BillDmsPaid { get; set; }
        public DbSet<TCMReferralFormEntity> TCMReferralForms { get; set; }
        public DbSet<TCMSupervisionTimeEntity> TCMSupervisionTimes { get; set; }
        public DbSet<TCMSubServiceEntity> TCMSubServices { get; set; }
        public DbSet<TCMTransferEntity> TCMTransfers { get; set; }
        public DbSet<IntakeConsentForTelehealthEntity> IntakeConsentForTelehealth { get; set; }
        public DbSet<IntakeNoDuplicateServiceEntity> IntakeNoDuplicateService { get; set; }
        public DbSet<IntakeAdvancedDirectiveEntity> IntakeAdvancedDirective { get; set; }
        public DbSet<ReferralFormEntity> ReferralForms { get; set; }
        public DbSet<TCMIntakeAppendixIEntity> TCMIntakeAppendixI { get; set; }
        public DbSet<IntakeClientIdDocumentVerificationEntity> IntakeClientDocumentVerification { get; set; }
        public DbSet<IntakeForeignLanguageEntity> IntakeForeignLanguage { get; set; }
        public DbSet<SafetyPlanEntity> SafetyPlan { get; set; }
        public DbSet<IncidentReportEntity> IncidentReport { get; set; }
        public DbSet<MeetingNoteEntity> MeetingNotes { get; set; }
        public DbSet<MeetingNotes_Facilitator> MeetingNotes_Facilitators { get; set; }
        public DbSet<TCMPayStubEntity> TCMPayStubs { get; set; }
        public DbSet<TCMPayStubDetailsEntity> TCMPayStubDetails { get; set; }
        public DbSet<TCMIntakeMedicalHistoryEntity> TCMIntakeMedicalHistory { get; set; }
        public DbSet<IntakeNoHarmEntity> IntakeNoHarm { get; set; }
        public DbSet<CourseEntity> Courses { get; set; }
        public DbSet<CaseManagerCertificationEntity> CaseManagerCertifications { get; set; }
        public DbSet<TCMSupervisorCertificationEntity> TCMSupervisorCertifications { get; set; }
        public DbSet<PromotionEntity> Promotions { get; set; }
        public DbSet<PromotionPhotosEntity> PromotionPhotos { get; set; }
        public DbSet<TCMSubServiceStepEntity> TCMSubServiceSteps { get; set; }
        public DbSet<FacilitatorCertificationEntity> FacilitatorCertifications { get; set; }
        public DbSet<DocumentAssistantCertificationEntity> DocumentAssistantCertifications { get; set; }
        public DbSet<SupervisorCertificationEntity> SupervisorCertifications { get; set; }
        public DbSet<NotePSYEntity> NotesPSYs { get; set; }

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
                        .HasIndex(s => s.Code)
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
                        .HasOne(c => c.TcmInterventionLog)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeInterventionLogEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMIntakeInterventionEntity>()
                        .HasOne(g => g.TcmInterventionLog)
                        .WithMany(m => m.InterventionList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMAssessment)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMAssessmentEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMAssessmentIndividualAgencyEntity>()
                        .HasOne(g => g.TcmAssessment)
                        .WithMany(m => m.IndividualAgencyList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMAssessmentHouseCompositionEntity>()
                        .HasOne(g => g.TcmAssessment)
                        .WithMany(m => m.HouseCompositionList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMAssessmentPastCurrentServiceEntity>()
                        .HasOne(g => g.TcmAssessment)
                        .WithMany(m => m.PastCurrentServiceList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMAssessmentMedicationEntity>()
                        .HasOne(g => g.TcmAssessment)
                        .WithMany(m => m.MedicationList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMFarsFormEntity>()
                        .HasOne(g => g.TCMClient)
                        .WithMany(m => m.TCMFarsFormList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMAssessmentHospitalEntity>()
                   .HasOne(g => g.TcmAssessment)
                   .WithMany(m => m.HospitalList)
                   .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMAssessmentDrugEntity>()
                        .HasOne(g => g.TcmAssessment)
                        .WithMany(m => m.DrugList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMAssessmentMedicalProblemEntity>()
                        .HasOne(g => g.TcmAssessment)
                        .WithMany(m => m.MedicalProblemList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMAssessmentSurgeryEntity>()
                        .HasOne(g => g.TcmAssessment)
                        .WithMany(m => m.SurgeryList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMNoteEntity>()
                        .HasMany(wd => wd.TCMNoteActivity)
                        .WithOne(n => n.TCMNote)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentsAssistantEntity>()
                       .HasIndex(s => s.Name)
                       .IsUnique();

            modelBuilder.Entity<TCMServiceEntity>()
                        .HasMany(c => c.TCMServiceActivity)
                        .WithOne(s => s.TcmService)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Client_Referred>()
                       .HasOne(cd => cd.Client)
                       .WithMany(c => c.Client_Referred)
                       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ObjectiveTempEntity>()
                       .HasOne(o => o.GoalTemp)
                       .WithMany(g => g.ObjetiveTempList)
                       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.Brief)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<BriefEntity>(s => s.Client_FK);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.GroupNote2)
                        .WithOne(gn => gn.Workday_Cient)
                        .HasForeignKey<GroupNote2Entity>(g => g.Workday_Client_FK);

            modelBuilder.Entity<Workday_Client>()
                        .HasOne(wd => wd.GroupNote2)
                        .WithOne(n => n.Workday_Cient)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMClientEntity>()
                       .HasOne(c => c.TCMIntakeClientSignatureVerification)
                       .WithOne(s => s.TcmClient)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<TCMIntakeClientSignatureVerificationEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                       .HasOne(c => c.TCMIntakeClientIdDocumentVerification)
                       .WithOne(s => s.TcmClient)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<TCMIntakeClientIdDocumentVerificationEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                      .HasOne(c => c.TCMIntakePainScreen)
                      .WithOne(s => s.TcmClient)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasForeignKey<TCMIntakePainScreenEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                      .HasOne(c => c.TCMIntakeColumbiaSuicide)
                      .WithOne(s => s.TcmClient)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasForeignKey<TCMIntakeColumbiaSuicideEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                      .HasOne(c => c.TCMIntakeNutritionalScreen)
                      .WithOne(s => s.TcmClient)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasForeignKey<TCMIntakeNutritionalScreenEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                      .HasOne(c => c.TCMIntakePersonalWellbeing)
                      .WithOne(s => s.TcmClient)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasForeignKey<TCMIntakePersonalWellbeingEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMReferralForm)
                        .WithOne(s => s.TcmClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMReferralFormEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<ClientEntity>()
                       .HasOne(c => c.IntakeConsentForTelehealth)
                       .WithOne(s => s.Client)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<IntakeConsentForTelehealthEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                      .HasOne(c => c.IntakeNoDuplicateService)
                      .WithOne(s => s.Client)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasForeignKey<IntakeNoDuplicateServiceEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                      .HasOne(c => c.IntakeAdvancedDirective)
                      .WithOne(s => s.Client)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasForeignKey<IntakeAdvancedDirectiveEntity>(s => s.Client_FK);
            
            modelBuilder.Entity<ClientEntity>()
                        .HasOne(c => c.ReferralForm)
                        .WithOne(s => s.Client)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<ReferralFormEntity>(s => s.Client_FK);

            modelBuilder.Entity<TCMClientEntity>()
                       .HasOne(c => c.TcmIntakeAppendixI)
                       .WithOne(s => s.TcmClient)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<TCMIntakeAppendixIEntity>(s => s.TcmClient_FK);

            modelBuilder.Entity<ClientEntity>()
                       .HasOne(c => c.IntakeForeignLanguage)
                       .WithOne(s => s.Client)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<IntakeForeignLanguageEntity>(s => s.Client_FK);

            modelBuilder.Entity<ClientEntity>()
                      .HasOne(c => c.IntakeClientIdDocumentVerification)
                      .WithOne(s => s.Client)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasForeignKey<IntakeClientIdDocumentVerificationEntity>(s => s.Client_FK);

            modelBuilder.Entity<IncidentReportEntity>()
                         .HasOne(g => g.Client)
                         .WithMany(m => m.IncidentReport)
                         .OnDelete(DeleteBehavior.Cascade);

             modelBuilder.Entity<MeetingNotes_Facilitator>()
                        .HasOne(cd => cd.MeetingNoteEntity)
                        .WithMany(c => c.FacilitatorList)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TCMClientEntity>()
                        .HasOne(c => c.TCMIntakeMedicalHistory)
                        .WithOne(s => s.TCMClient)
                        .OnDelete(DeleteBehavior.Cascade)
                        .HasForeignKey<TCMIntakeMedicalHistoryEntity>(s => s.TCMClient_FK);

            modelBuilder.Entity<ClientEntity>()
                       .HasOne(c => c.IntakeNoHarm)
                       .WithOne(s => s.Client)
                       .OnDelete(DeleteBehavior.Cascade)
                       .HasForeignKey<IntakeNoHarmEntity>(s => s.Client_FK);
        }
    }
}
