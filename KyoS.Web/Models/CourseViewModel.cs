using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class CourseViewModel : CourseEntity
    {
        [Display(Name = "Role")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Role.")]
        public int IdRole { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; }

        
    }
}
