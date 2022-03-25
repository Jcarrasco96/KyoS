namespace KyoS.Web.Models
{
    public class TCMDomainObjetiveReview
    {
        public int ID { get; set; }
        public int Status { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Recomendation { get; set; }
        public TCMObjetiveReview []ObjectiveList { get; set; }
    }
}