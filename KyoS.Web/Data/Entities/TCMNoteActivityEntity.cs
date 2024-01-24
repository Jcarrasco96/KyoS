using KyoS.Web.Data.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMNoteActivityEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMNoteEntity TCMNote { get; set; }

        public string Setting { get; set; }

        public TCMDomainEntity TCMDomain { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
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

        public TCMServiceActivityEntity TCMServiceActivity { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ServiceName { get; set; }
        public bool Billable { get; set; }
    }
}
