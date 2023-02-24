using KyoS.Web.Data.Contracts;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class SubScheduleEntity : AuditableEntity
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

        public ScheduleEntity Schedule { get; set; }
       
        
    }
}
