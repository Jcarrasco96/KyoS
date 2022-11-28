using KyoS.Web.Data.Entities;

namespace KyoS.Web.Models
{
    public class Workday_ClientViewModel : Workday_Client
    {
        public int Origin { get; set; }
        public string DateInterval { get; set; }
    }
}
