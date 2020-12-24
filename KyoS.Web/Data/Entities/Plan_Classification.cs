namespace KyoS.Web.Data.Entities
{
    public class Plan_Classification
    {
        public int Id { get; set; }

        public PlanEntity Plan { get; set; }

        public ClassificationEntity Classification { get; set; }
    }
}
