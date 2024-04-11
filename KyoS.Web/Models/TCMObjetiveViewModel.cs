using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMObjetiveViewModel : TCMObjetiveEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Stage")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Stage.")]
        public int Id_Stage { get; set; }
        public IEnumerable<SelectListItem> Stages { get; set; }

        public string name { get; set; }

        public string task { get; set; }

        public string descriptionStages { get; set; }

        //este campo lo uso para saber de que pagina se viene
        public int Origi { get; set; }

        public int Id_Domain { get; set; }

        public int IdServicePlanReview { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ChangesUpdates { get; set; }

        //[Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        public int Idd { get; set; }
    }
}
