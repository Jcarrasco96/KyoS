using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class GroupNote2_Activity
    {
        public int Id { get; set; }

        public GroupNote2Entity GroupNote2 { get; set; }

        public ActivityEntity Activity { get; set; }

        [Display(Name = "Client's Answer")]
        public string AnswerClient { get; set; }

        [Display(Name = "Facilitator's Answer")]
        public string AnswerFacilitator { get; set; }

        public ObjetiveEntity Objetive { get; set; }
        public SubScheduleEntity SubSchedule { get; set; }
    }
}
