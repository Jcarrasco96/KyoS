using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class SettingViewModel : SettingEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]
        public int IdClinic { get; set; }

        public IEnumerable<SelectListItem> Clinics { get; set; }

        public int IdFiltroPayStub { get; set; }

        public IEnumerable<SelectListItem> FiltroPayStubs { get; set; }

        [Display(Name = "Dashboard Principal")]
        public int IdDashboard { get; set; }

        public IEnumerable<SelectListItem> Dashboards { get; set; }
    }
}
