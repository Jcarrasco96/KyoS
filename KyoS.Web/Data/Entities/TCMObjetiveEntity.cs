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

        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "ID_Objetive")]
        public int IdObjetive { get; set; }

        public string Task { get; set; }
        public StatusType Status { get; set; }
        public string Responsible { get; set; }
       

        public bool Finish { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime StartDate{ get; set; }

        [Display(Name = "Target Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime TargetDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public TCMDomainEntity TcmDomain { get; set; }

        public string Origin { get; set; }

    }
}
