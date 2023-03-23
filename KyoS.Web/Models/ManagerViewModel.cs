using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ManagerViewModel : ManagerEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]
        public int IdClinic { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        public int IdStatus { get; set; }
        
        [Display(Name = "Linked user")]        
        public string IdUser { get; set; }

        [Display(Name = "Signature")]
        public IFormFile SignatureFile { get; set; }

        public IEnumerable<SelectListItem> UserList { get; set; }

        public IEnumerable<SelectListItem> StatusList { get; set; }

        public IEnumerable<SelectListItem> Clinics { get; set; }
    }
}
