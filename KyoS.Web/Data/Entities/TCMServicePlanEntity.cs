using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMServicePlanEntity : AuditableEntity
    {
        public int Id { get; set; }

        public int TcmClient_FK { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateServicePlan{ get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateIntake { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateAssessment { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateCertification { get; set; }

        public string Strengths { get; set; }

        public string Weakness{ get; set; }

        public string DischargerCriteria { get; set; }

        public StatusType Status { get; set; }

        public int Approved { get; set; }

        public TCMClientEntity TcmClient { get; set; }
       
        public List <TCMDomainEntity> TCMDomain { get; set; }
        public List <TCMServiceEntity> TCMService { get; set; }
        public List<TCMAdendumEntity> TCMAdendum { get; set; }

        public TCMServicePlanReviewEntity TCMServicePlanReview { get; set; }

        public TCMDischargeEntity TCMDischarge { get; set; }
        public TCMSupervisorEntity TCMSupervisor { get; set; }
    }
}
