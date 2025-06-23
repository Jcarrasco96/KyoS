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
    public class TCMSupervisorCertificationViewModel : TCMSupervisorCertificationEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Course")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a course.")]
        public int IdCourse { get; set; }
        public IEnumerable<SelectListItem> Courses { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "TCM")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a TCM Supervisors.")]
        public int IdTCM { get; set; }

        public IEnumerable<SelectListItem> TCMSupervisors { get; set; }

       
    }
}

