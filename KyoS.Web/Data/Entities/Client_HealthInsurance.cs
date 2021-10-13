using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class Client_HealthInsurance
    {
        public int Id { get; set; }

        public DateTime ApprovedDate { get; set; }

        public DateTime DurationTime { get; set; }

        public int Units { get; set; }

        public int UsedUnits { get; set; }

        public ClientEntity Client { get; set; }

        public HealthInsuranceEntity HealthInsurance { get; set; }
    }
}
