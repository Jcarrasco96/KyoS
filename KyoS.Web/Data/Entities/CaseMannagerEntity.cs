using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class CaseMannagerEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Code")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public StatusType Status { get; set; }

        public string LinkedUser { get; set; }

        [Display(Name = "Signature")]
        public string SignaturePath { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }

        public ClinicEntity Clinic { get; set; }
    }
}
