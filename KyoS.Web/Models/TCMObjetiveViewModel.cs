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
        /*[Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]
        public int IdClinic { get; set; }*/

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Stage")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Stage.")]
        public int Id_Stage { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int ID_Objetive { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string name { get; set; }

        public string task { get; set; }
        //public string long_Term { get; set; }

        public DateTime Start_Date { get; set; }
        public DateTime Target_Date { get; set; }
        public DateTime End_Date { get; set; }
        public string descriptionStages { get; set; }

        //este campo lo uso para saber de que pagina se viene
        public int Origin { get; set; }

        public int Id_Domain { get; set; }
        // public IEnumerable<SelectListItem> Clinics { get; set; }
        public IEnumerable<SelectListItem> Stages { get; set; }

        public int IdServicePlanReview { get; set; }

        public string ChangesUpdates { get; set; }

        //[Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        public int Idd { get; set; }
    }
}
