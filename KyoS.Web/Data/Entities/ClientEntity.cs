using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ClientEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public GenderType Gender { get; set; }

        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = false)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "Medical ID")]
        public string MedicalID { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public StatusType Status { get; set; }

        public ClinicEntity Clinic { get; set; }

        public GroupEntity Group { get; set; }

        public ICollection<Workday_Client> Workdays_Clients { get; set; }
    }
}
