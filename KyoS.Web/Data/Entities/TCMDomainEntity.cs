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

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Code")]
        public string Code { get; set; }

        public string NeedsIdentified { get; set; }
        public string LongTerm { get; set; }
        public DateTime DateIdentified { get; set; }
        
        public TCMServicePlanEntity TcmServicePlan { get; set; }
        public List <TCMObjetiveEntity> TCMObjetive { get; set; }
        
        public string Origin { get; set; }

        public bool Used { get; set; }
        public bool Status { get; set; }

    }
}
