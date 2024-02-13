using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMDomainEntity : AuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string NeedsIdentified { get; set; }
        public string LongTerm { get; set; }

        [Display(Name = "Date Identified")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateIdentified { get; set; }
        
        public TCMServicePlanEntity TcmServicePlan { get; set; }
        public List <TCMObjetiveEntity> TCMObjetive { get; set; }
        
        public string Origin { get; set; }

        public bool Used { get; set; }
        public bool Status { get; set; }

        public int IdSubService { get; set; }
        public string NameSubService { get; set; }

        [Display(Name = "Date Accomplished")]
        [DataType(DataType.Date)]
        public DateTime DateAccomplished { get; set; }
    }
}
