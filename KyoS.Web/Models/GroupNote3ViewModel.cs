using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class GroupNote3ViewModel : GroupNote2Entity
    {
        public int IdActivity1 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Client input content")]
        public string AnswerClient1 { get; set; }

        //[Required(ErrorMessage = "The field {0} is mandatory.")]
        //[Display(Name = "Facilitator statement #1")]
       // public string AnswerFacilitator1 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal1 { get; set; }
        public IEnumerable<SelectListItem> Goals1 { get; set; }

        [Display(Name = "Objective")]
        public int IdObjetive1 { get; set; }
        public IEnumerable<SelectListItem> Objetives1 { get; set; }

        public string Intervention1 { get; set; }

        //este campo lo uso para saber de que pagina se viene
        public int Origin { get; set; }

        //estos campos solo se usan para el modo solo lectura
        public string Topic1 { get; set; }
        public string Activity1 { get; set; }
        public string Goal1 { get; set; }
        public string Objetive1 { get; set; }
       
        public string CodeBill { get; set; }

        public int Minute1 { get; set; }
    }
}
