using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ClinicEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Logo")]
        public string LogoPath { get; set; }

        public ICollection<FacilitatorEntity> Facilitators { get; set; }
        public ICollection<SupervisorEntity> Supervisors { get; set; }
        public ICollection<ThemeEntity> Themes { get; set; }
        public ICollection<ClientEntity> Clients { get; set; }
        public ICollection<UserEntity> Users { get; set; }

    }
}
