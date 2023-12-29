using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class SafetyPlanEntity : AuditableEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureClient { get; set; }

        [Display(Name = "Date of Facilitator Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureFacilitator { get; set; }

        public bool Documents { get; set; }

        public FacilitatorEntity Facilitator { get; set; }

        public string PeopleIcanCall { get; set; }
        public string WaysToKeepmyselfSafe { get; set; }
        public string AdviceIwould { get; set; }
        public string WaysToDistract { get; set; }
        public string WarningSignsOfCrisis { get; set; }
        public string ThingsThat { get; set; }

        public SafetyPlanStatus Status { get; set; }
        public SupervisorEntity Supervisor { get; set; }

        [Display(Name = "Date of Safety Plan")]
        [DataType(DataType.Date)]

        public DateTime DateDocument { get; set; }

        public DocumentsAssistantEntity DocumentAssisstant { get; set; }
    }
}
