using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ScheduleViewModel : ScheduleEntity
    {
        [Display(Name = "Session")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a Session.")]
        public int IdSession { get; set; }
        public IEnumerable<SelectListItem> Sessions { get; set; }

        [Display(Name = "Service")]
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a Service.")]
        public int IdService { get; set; }
        public IEnumerable<SelectListItem> Services { get; set; }
    }
}
