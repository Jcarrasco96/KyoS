using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class CiteViewModel : CiteEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]
        public int IdClinic { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        public int IdStatus { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Client")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a client.")]
        public int IdClient { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Schedule")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a schedule.")]
        public int IdSchedule { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Facilitator")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a facilitator.")]
        public int IdFacilitator { get; set; }

        public IEnumerable<SelectListItem> ClientsList { get; set; }

        public IEnumerable<SelectListItem> StatusList { get; set; }

        public IEnumerable<SelectListItem> Clinics { get; set; }

        public IEnumerable<SelectListItem> FacilitatorsList { get; set; }

        public IEnumerable<SelectListItem> SchedulesList { get; set; }

        public string Service { get; set; }
    }
}
