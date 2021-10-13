using KyoS.Web.Data.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class HealthInsuranceEntity : AuditableEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Signed date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime SignedDate { get; set; }

        [Display(Name = "Duration time (months)")]
        public int? DurationTime { get; set; }

        public string DocumentPath { get; set; }

        public bool Active { get; set; }

        public ClinicEntity Clinic { get; set; }
    }
}
