using KyoS.Web.Data.Contracts;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class ScheduleEntity : AuditableEntity
    {
        public int Id { get; set; }

        [Display(Name = "Initial Time")]
        [DataType(DataType.Time)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime InitialTime { get; set; }

        [Display(Name = "Diagnostic")]
        [DataType(DataType.Time)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime EndTime { get; set; }

        public string Session { get; set; }
        public ServiceType Service { get; set; }
        public string Description { get; set; }

        public ICollection<SubScheduleEntity> SubSchedules { get; set; }
        public ClinicEntity Clinic { get; set; }
    }
}
