using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class FacilitatorCertificationViewModel : FacilitatorCertificationEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Course")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a course.")]
        public int IdCourse { get; set; }
        public IEnumerable<SelectListItem> Courses { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Facilitator")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Facilitator.")]
        public int IdFacilitator { get; set; }

        public IEnumerable<SelectListItem> Facilitators { get; set; }

       
    }
}

