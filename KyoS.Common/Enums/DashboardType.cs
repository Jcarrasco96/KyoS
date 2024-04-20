namespace KyoS.Common.Enums
{
    public enum DashboardType
    {
        MH,
        TCM
        
    }
    public class DashboardUtils
    {
        public static DashboardType GetDashboardTypeByIndex(int index)
        {
            return (index == 0) ? DashboardType.MH :
                   (index == 1) ? DashboardType.TCM : DashboardType.MH;
        }
    }
  
}
