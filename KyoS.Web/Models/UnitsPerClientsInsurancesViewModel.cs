namespace KyoS.Web.Models
{
    public class UnitsPerClientsInsurancesViewModel
    {
        public string ClientName { get; set; }
        public string ClientCode { get; set; }
        public string HealthInsuranceName { get; set; }
        public int ApprovedUnits { get; set; }
        public int UsedUnits { get; set; }
        public int AvailableUnits { get; set; }
    }
}
