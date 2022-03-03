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
    public class TCMObjetiveViewModel : TCMObjetiveEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]
        public int IdClinic { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Units")]
        public int units { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int ID_Objetive { get; set; }

        public string description { get; set; }

        public DateTime Start_Date { get; set; }
        public DateTime Target_Date { get; set; }
        public DateTime End_Date { get; set; }

        public int Id_Domain { get; set; }
        public IEnumerable<SelectListItem> Clinics { get; set; }
    }
}
