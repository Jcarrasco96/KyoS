using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMServicePlanReviewDomainObjectiveEntity : AuditableEntity
    {
        public int Id { get; set; }

        public int IdObjective { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{mm/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime DateEndObjective { get; set; }

        public StatusType Status { get; set; }

        public string Origin { get; set; }

        public TCMServicePlanReviewDomainEntity tcmServicePlanReviewDomain { get; set; }

        public string Name { get; set; }

        public string Task { get; set; }
        public string Responsible { get; set; }
        public bool Finish { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Target Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime TargetDate { get; set; }

        
    }
}
