using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class Workday_Activity_Facilitator
    {
        public int Id { get; set; }
        public WorkdayEntity Workday { get; set; }
        public ActivityEntity Activity { get; set; }
        public FacilitatorEntity Facilitator { get; set; }
        public SchemaType Schema { get; set; }
        public bool? copingSkills { get; set; }
        public bool? stressManagement { get; set; }
        public bool? healthyLiving { get; set; }
        public bool? relaxationTraining { get; set; }
        public bool? diseaseManagement { get; set; }
        public bool? communityResources { get; set; }
        public bool? activityDailyLiving { get; set; }
        public bool? socialSkills { get; set; }
        public bool? lifeSkills { get; set; }
    }
}
