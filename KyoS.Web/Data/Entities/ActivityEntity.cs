using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ActivityEntity
    {
        public int Id { get; set; }

        [Display(Name = "Activity")]
        [MaxLength(1000, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        public ThemeEntity Theme { get; set; }

        public ICollection<Note_Activity> Notes_Activities { get; set; }

        public ICollection<Workday_Activity_Facilitator> Workdays_Activities_Facilitators { get; set; }
    }
}
