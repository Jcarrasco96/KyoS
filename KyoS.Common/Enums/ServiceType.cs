namespace KyoS.Common.Enums
{
    public enum ServiceType
    {
        PSR,
        Individual,
        Group
    }

    public class ServiceUtils
    {
        public static ServiceType GetServiceByIndex(int index)
        {
            return (index == 0) ? ServiceType.PSR :
                   (index == 1) ? ServiceType.Individual :
                   (index == 2) ? ServiceType.Group : ServiceType.PSR;
        }
    }

    public enum ServiceAgency
    {
        CMH,
        TCM
    }

    public class ServiceAgencyUtils
    {
        public static ServiceAgency GetServiceAgencyByIndex(int index)
        {
            return (index == 0) ? ServiceAgency.CMH :
                   (index == 1) ? ServiceAgency.TCM : ServiceAgency.CMH;
        }
    }
}
