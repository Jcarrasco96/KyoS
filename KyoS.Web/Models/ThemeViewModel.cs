using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ThemeViewModel : ThemeEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Day")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a day.")]
        public int DayId { get; set; }
        public IEnumerable<SelectListItem> Days { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]
        public int IdClinic { get; set; }
        public IEnumerable<SelectListItem> Clinics { get; set; }
    }
}
