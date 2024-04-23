using KyoS.Web.Data.Contracts;
using System;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class Client_HealthInsurance : AuditableEntity
    {
        public int Id { get; set; }

        [Display(Name = "Approved Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime ApprovedDate { get; set; }

        [Display(Name = "Duration time (months)")]
        public int DurationTime { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Units { get; set; }

        public bool Active { get; set; }

        public ClientEntity Client { get; set; }

        public HealthInsuranceEntity HealthInsurance { get; set; }

        [Display(Name = "Member ID")]
        public string MemberId { get; set; }

        [Display(Name = "Authorization Number")]
        public string AuthorizationNumber { get; set; }

        public ServiceAgency Agency { get; set; }

        [Display(Name = "Expired Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime ExpiredDate { get; set; }
        public int UsedUnits { get; set; }

        [Display(Name = "Insurance Effective Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime EffectiveDate { get; set; }

        [Display(Name = "Insurance End Coverage Date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime EndCoverageDate { get; set; }

        [Display(Name = "Insurance Type")]
        public InsuranceType InsuranceType { get; set; }

        [Display(Name = "Plan")]
        public InsurancePlanType InsurancePlan { get; set; }

        [Display(Name = "Coverage")]
        public InsuranceCoverageType InsuranceCoverage { get; set; }
    }
}
