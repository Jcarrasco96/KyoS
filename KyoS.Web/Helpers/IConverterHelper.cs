using KyoS.Web.Data.Entities;
using KyoS.Web.Models;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public interface IConverterHelper
    {
        ClinicEntity ToClinicEntity(ClinicViewModel model, string path, bool isNew);
        ClinicViewModel ToClinicViewModel(ClinicEntity model);
        Task<ThemeEntity> ToThemeEntity(ThemeViewModel model, bool isNew);
        Task<ThemeEntity> ToTheme3Entity(Theme3ViewModel model, bool isNew);
        ThemeViewModel ToThemeViewModel(ThemeEntity model);
        Theme3ViewModel ToTheme3ViewModel(ThemeEntity model);
        Task<ActivityEntity> ToActivityEntity(ActivityViewModel model, bool isNew);
        ActivityViewModel ToActivityViewModel(ActivityEntity model);
        Task<NotePrototypeEntity> ToNotePrototypeEntity(NotePrototypeViewModel model, bool isNew);
        NotePrototypeViewModel ToNotePrototypeViewModel(NotePrototypeEntity model);
        Task<FacilitatorEntity> ToFacilitatorEntity(FacilitatorViewModel model, string signaturePath, bool isNew);
        FacilitatorViewModel ToFacilitatorViewModel(FacilitatorEntity model, int idClinic);
        Task<ClientEntity> ToClientEntity(ClientViewModel model, bool isNew, string photoPath, string signPath, string userId);
        ClientViewModel ToClientViewModel(ClientEntity model, string userId);
        Task<SupervisorEntity> ToSupervisorEntity(SupervisorViewModel model, string signaturePath, bool isNew);
        SupervisorViewModel ToSupervisorViewModel(SupervisorEntity model, int idClinic);
        Task<MTPEntity> ToMTPEntity(MTPViewModel model, bool isNew);
        MTPViewModel ToMTPViewModel(MTPEntity model);       
        Task<GoalEntity> ToGoalEntity(GoalViewModel model, bool isNew);
        GoalViewModel ToGoalViewModel(GoalEntity model);
        Task<ObjetiveEntity> ToObjectiveEntity(ObjectiveViewModel model, bool isNew);
        ObjectiveViewModel ToObjectiveViewModel(ObjetiveEntity model);
        Task<GroupEntity> ToGroupEntity(GroupViewModel model, bool isNew);
        GroupViewModel ToGroupViewModel(GroupEntity model);
        Task<PlanEntity> ToPlanEntity(PlanViewModel model, bool isNew);
        PlanViewModel ToPlanViewModel(PlanEntity model);
        Task<NoteEntity> ToNoteEntity(NoteViewModel model, bool isNew);
        NoteViewModel ToNoteViewModel(NoteEntity model);
        Task<NotePEntity> ToNotePEntity(NotePViewModel model, bool isNew);
        NotePViewModel ToNotePViewModel(NotePEntity model);
        Task<IndividualNoteEntity> ToIndividualNoteEntity(IndividualNoteViewModel model, bool isNew);
        Task<GroupNoteEntity> ToGroupNoteEntity(GroupNoteViewModel model, bool isNew);       
        Workday_ClientViewModel ToWorkdayClientViewModel(Workday_Client model);
        Task<MessageEntity> ToMessageEntity(MessageViewModel model, bool isNew);
        DoctorEntity ToDoctorEntity(DoctorViewModel model, bool isNew, string userId);
        PsychiatristEntity ToPsychiatristEntity(PsychiatristViewModel model, bool isNew, string userId);
        LegalGuardianEntity ToLegalGuardianEntity(LegalGuardianViewModel model, bool isNew, string userId);
        EmergencyContactEntity ToEmergencyContactEntity(EmergencyContactViewModel model, bool isNew, string userId);
        ReferredEntity ToReferredEntity(ReferredViewModel model, bool isNew, string userId);
        DiagnosticEntity ToDiagnosticEntity(DiagnosticViewModel model, bool isNew, string userId);
        DoctorViewModel ToDoctorViewModel(DoctorEntity model);
        PsychiatristViewModel ToPsychiatristViewModel(PsychiatristEntity model);
        ReferredViewModel ToReferredViewModel(ReferredEntity model);
        LegalGuardianViewModel ToLegalGuardianViewModel(LegalGuardianEntity model);
        EmergencyContactViewModel ToEmergencyContactViewModel(EmergencyContactEntity model);
        DiagnosticViewModel ToDiagnosticViewModel(DiagnosticEntity model);
        Task<IncidentEntity> ToIncidentEntity(IncidentViewModel model, bool isNew, string userId);
        IncidentViewModel ToIncidentViewModel(IncidentEntity model);
        Task<HealthInsuranceEntity> ToHealthInsuranceEntity(HealthInsuranceViewModel model, bool isNew, string userId, string documentPath);
        HealthInsuranceViewModel ToHealthInsuranceViewModel(HealthInsuranceEntity model);
        Task<Client_HealthInsurance> ToClientHealthInsuranceEntity(UnitsAvailabilityViewModel model, bool isNew, string userId);
        UnitsAvailabilityViewModel ToClientHealthInsuranceViewModel(Client_HealthInsurance model,  int idClinic);
        Task<SettingEntity> ToSettingEntity(SettingViewModel model, bool isNew, string userId);
        SettingViewModel ToSettingViewModel(SettingEntity model);
    }
}
