using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class Workday_Activity_Facilitator
    {
        public int Id { get; set; }
        public WorkdayEntity Workday { get; set; }
        public ActivityEntity Activity { get; set; }
        public FacilitatorEntity Facilitator { get; set; }
    }
}
