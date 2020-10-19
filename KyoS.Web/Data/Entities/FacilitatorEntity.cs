using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class FacilitatorEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Firm")]
        public string Firm { get; set; }

        public ClinicEntity Clinic { get; set; }
    }
}
