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
        [DisplayFormat(DataFormatString = "{mm/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime DateEndObjective { get; set; }

        public StatusType Status { get; set; }

        public string Origin { get; set; }

        public string ChangesUpdate { get; set; }

        public TCMServicePlanReviewDomainEntity tcmServicePlanReviewDomain { get; set; }
    }
}
