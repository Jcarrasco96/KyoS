using KyoS.Web.Data.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

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
        
        public string MemberId { get; set; }
    }
}
