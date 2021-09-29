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
    }
}
