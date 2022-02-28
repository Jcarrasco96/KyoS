using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMStageViewModel : TCMStageEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]

        public int IdClinic { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "ID_TCMService")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]

        public int Id_TCMService{ get; set; }
        public IEnumerable<SelectListItem> Clinics { get; set; }
    }
}
