using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMServicePlanReviewEntity : AuditableEntity
    {
        public int Id { get; set; }

        public int TcmServicePlan_FK { get; set; }

        [Display(Name = "Date Service Plan Review")]
        [DataType(DataType.Date)]
        public DateTime DateServicePlanReview { get; set; }

        [Display(Name = "Intake Date")]
        [DataType(DataType.Date)]
        public DateTime DateOpending { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string SummaryProgress { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Recomendation { get; set; }

        public int Approved { get; set; }

        public TCMServicePlanEntity TcmServicePlan { get; set; }
        public TCMSupervisorEntity TCMSupervisor { get; set; }
        public List<TCMServicePlanReviewDomainEntity> TCMServicePlanRevDomain { get; set; }
        public IEnumerable<TCMMessageEntity> TCMMessages { get; set; }

        public bool ClientHasBeen1 { get; set; }
        public bool ClientContinue { get; set; }
        public bool ClientNoLonger1 { get; set; }
        public bool ClientHasBeen2 { get; set; }
        public bool ClientWillContinue { get; set; }
        public bool ClientWillHave { get; set; }
        public bool ClientNoLonger2 { get; set; }

        public bool HasBeenExplained { get; set; }

        [DataType(DataType.Date)]
        public DateTime TheExpertedReviewDate { get; set; }

        [Display(Name = "Date Case Manager Signature")]
        [DataType(DataType.Date)]
        public DateTime DateTCMCaseManagerSignature { get; set; }

        [Display(Name = "Date TCM Supervisor Signature")]
        [DataType(DataType.Date)]
        public DateTime DateTCMSupervisorSignature { get; set; }

        [Display(Name = "Assessment Date")]
        [DataType(DataType.Date)]
        public DateTime DateAssessment { get; set; }

        [Display(Name = "Certification Date")]
        [DataType(DataType.Date)]
        public DateTime DateCertification { get; set; }
    }
}
