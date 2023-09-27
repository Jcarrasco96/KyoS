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
        
    }
}
