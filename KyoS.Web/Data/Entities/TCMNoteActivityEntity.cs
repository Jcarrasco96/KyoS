using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMNoteActivityEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMNoteEntity TCMNote { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Setting { get; set; }
       
        public TCMDomainEntity TCMDomain { get; set; }

        public string DescriptionOfService { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime EndTime { get; set; }

        public int Minutes { get; set; }

    }
}
