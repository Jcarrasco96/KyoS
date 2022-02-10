using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ThemeEntity
    {
        public int Id { get; set; }

        [Display(Name = "Topic")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Day of week")]
        public DayOfWeekType? Day { get; set; }

        public ClinicEntity Clinic { get; set; }
    }
}
