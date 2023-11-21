using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMSupervisionTimeEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMSupervisorEntity TCMSupervisor { get; set; }
        public CaseMannagerEntity CaseManager { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateSupervision { get; set; }

        [DataType(DataType.Time)]
        public DateTime StartTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime EndTime { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
