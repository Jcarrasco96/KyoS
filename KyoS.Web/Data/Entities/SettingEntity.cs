using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class SettingEntity : AuditableEntity
    {
        public int Id { get; set; }
        public int Clinic_FK { get; set; }
        public ClinicEntity Clinic { get; set; }
        public bool AvailableCreateNewWorkdays { get; set; }
        public bool MentalHealthClinic { get; set; }
        public bool TCMClinic { get; set; }
        public bool MHClassificationOfGoals { get; set; }
        public bool MHProblems { get; set; }
        public bool TCMSupervisorEdit { get; set; }
    }
}
