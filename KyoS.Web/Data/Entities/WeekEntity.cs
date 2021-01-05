using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class WeekEntity
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]        
        public DateTime InitDate { get; set; }
        [DataType(DataType.Date)]        
        public DateTime FinalDate { get; set; }
        public IEnumerable<WorkdayEntity> Days { get; set; }
        public ClinicEntity Clinic { get; set; }
        public int WeekOfYear { get; set; }
    }
}
