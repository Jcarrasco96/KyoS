namespace KyoS.Web.Data.Entities
{
    public class Objetive_Classification
    {
        public int Id { get; set; }

        public ObjetiveEntity Objetive { get; set; }

        public ClassificationEntity Classification { get; set; }
    }
}
