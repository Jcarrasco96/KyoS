using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMObjetiveEntity : AuditableEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "ID_Objetive")]
        public int IdObjetive { get; set; }

        public string Task { get; set; }
        public StatusType Status { get; set; }
        public string Responsible { get; set; }
       

        public bool Finish { get; set; }
        public DateTime StartDate{ get; set; }
        public DateTime TargetDate { get; set; }
        public DateTime EndDate { get; set; }

        public TCMDomainEntity TcmDomain { get; set; }

        public string Origin { get; set; }

    }
}
