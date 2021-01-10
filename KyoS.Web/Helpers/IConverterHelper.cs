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
        ThemeViewModel ToThemeViewModel(ThemeEntity model);
        Task<ActivityEntity> ToActivityEntity(ActivityViewModel model, bool isNew);
        ActivityViewModel ToActivityViewModel(ActivityEntity model);
        Task<NotePrototypeEntity> ToNotePrototypeEntity(NotePrototypeViewModel model, bool isNew);
        NotePrototypeViewModel ToNotePrototypeViewModel(NotePrototypeEntity model);
        Task<FacilitatorEntity> ToFacilitatorEntity(FacilitatorViewModel model, bool isNew);
        FacilitatorViewModel ToFacilitatorViewModel(FacilitatorEntity model, int idClinic);
        Task<ClientEntity> ToClientEntity(ClientViewModel model, bool isNew);
        ClientViewModel ToClientViewModel(ClientEntity model);
        Task<SupervisorEntity> ToSupervisorEntity(SupervisorViewModel model, bool isNew);
        SupervisorViewModel ToSupervisorViewModel(SupervisorEntity model);
        Task<MTPEntity> ToMTPEntity(MTPViewModel model, bool isNew);
        MTPViewModel ToMTPViewModel(MTPEntity model);
        Task<DiagnosisEntity> ToDiagnosisEntity(DiagnosisViewModel model, bool isNew);
        DiagnosisViewModel ToDiagnosisViewModel(DiagnosisEntity model);
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
    }
}
