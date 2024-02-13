using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMDateBlockedEntity : AuditableEntity
    {
        public int Id { get; set; }
        public int Clinic_FK { get; set; }
        public ClinicEntity Clinic { get; set; }

        [Display(Name = "Date Blocked")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateBlocked { get; set; }
       
        public string Description { get; set; }
        
    }
}
