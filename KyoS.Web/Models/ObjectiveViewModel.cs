using KyoS.Web.Data.Entities;

namespace KyoS.Web.Models
{
    public class ObjectiveViewModel : ObjetiveEntity
    {
        public int IdGoal { get; set; }
        public int IdMTPReviewActive { get; set; }
    }
}
