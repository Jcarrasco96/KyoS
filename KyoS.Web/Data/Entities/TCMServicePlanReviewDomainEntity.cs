using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMServicePlanReviewDomainEntity : AuditableEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ChangesUpdate { get; set; }

        public TCMDomainEntity TcmDomain { get; set; }
        public SPRStatus Status { get; set; }

        public List<TCMServicePlanReviewDomainObjectiveEntity> TCMServicePlanRevDomainObjectiive { get; set; }

        public string CodeDomain { get; set; }

        public TCMServicePlanReviewEntity TcmServicePlanReview { get; set; }

        public string Name { get; set; }
        public string NeedsIdentified { get; set; }
        public string LongTerm { get; set; }

        [Display(Name = "Date Identified")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateIdentified { get; set; }

        [Display(Name = "Date Accomplished")]
        [DataType(DataType.Date)]
        public DateTime DateAccomplished { get; set; }

    }
}
