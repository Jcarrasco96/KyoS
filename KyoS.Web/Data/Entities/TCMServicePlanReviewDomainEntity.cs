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
    }
}
