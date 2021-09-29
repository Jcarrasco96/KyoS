using KyoS.Common.Enums;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class NoteViewModel : NoteEntity
    {
        
        public int IdActivity1 { get; set; }
        
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Client Answer #1")]
        public string AnswerClient1 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Facilitator Answer #1")]
        public string AnswerFacilitator1 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal1 { get; set; }
        public IEnumerable<SelectListItem> Goals1 { get; set; }

        [Display(Name = "Objetive")]
        public int IdObjetive1 { get; set; }
        public IEnumerable<SelectListItem> Objetives1 { get; set; }

        public string Intervention1 { get; set; }

        public int IdActivity2 { get; set; }
        
        [Display(Name = "Client Answer #2")]
        public string AnswerClient2 { get; set; }

        [Display(Name = "Facilitator Answer #2")]
        public string AnswerFacilitator2 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal2 { get; set; }
        public IEnumerable<SelectListItem> Goals2 { get; set; }

        [Display(Name = "Objetive")]
        public int IdObjetive2 { get; set; }
        public IEnumerable<SelectListItem> Objetives2 { get; set; }
        
        public string Intervention2 { get; set; }

        public int IdActivity3 { get; set; }        

        [Display(Name = "Client Answer #3")]
        public string AnswerClient3 { get; set; }

        [Display(Name = "Facilitator Answer #3")]
        public string AnswerFacilitator3 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal3 { get; set; }
        public IEnumerable<SelectListItem> Goals3 { get; set; }

        [Display(Name = "Objetive")]
        public int IdObjetive3 { get; set; }
        public IEnumerable<SelectListItem> Objetives3 { get; set; }
        public string Intervention3 { get; set; }

        public int IdActivity4 { get; set; }        

        [Display(Name = "Client Answer #4")]
        public string AnswerClient4 { get; set; }

        [Display(Name = "Facilitator Answer #4")]
        public string AnswerFacilitator4 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal4 { get; set; }
        public IEnumerable<SelectListItem> Goals4 { get; set; }

        [Display(Name = "Objetive")]
        public int IdObjetive4 { get; set; }
        public IEnumerable<SelectListItem> Objetives4 { get; set; }
        public string Intervention4 { get; set; }

        //este campo lo uso para saber de que pagina se viene
        public int Origin { get; set; }

        //estos campos solo se usan para el modo solo lectura
        public string Topic1 { get; set; }        
        public string Activity1 { get; set; }
        public string Goal1 { get; set; }
        public string Objetive1 { get; set; }        
        public string Topic2 { get; set; }        
        public string Activity2 { get; set; }
        public string Goal2 { get; set; }
        public string Objetive2 { get; set; }        
        public string Topic3 { get; set; }        
        public string Activity3 { get; set; }
        public string Goal3 { get; set; }
        public string Objetive3 { get; set; }        
        public string Topic4 { get; set; }
        public string Activity4 { get; set; }
        public string Goal4 { get; set; }
        public string Objetive4 { get; set; }
    }
}
