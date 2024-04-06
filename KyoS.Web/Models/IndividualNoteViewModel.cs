using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class IndividualNoteViewModel : IndividualNoteEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a client.")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal1 { get; set; }
        public IEnumerable<SelectListItem> Goals1 { get; set; }

        [Display(Name = "Objective")]
        public int IdObjetive1 { get; set; }
        public IEnumerable<SelectListItem> Objetives1 { get; set; }        

        //este campo lo uso para saber de que pagina se viene
        public int Origin { get; set; }

        //estos campos solo se usan para el modo solo lectura
        public string Goal1 { get; set; }
        public string Objetive1 { get; set; }
        public string Intervention1 { get; set; }

        public string CodeBill { get; set; }
        public int IdSubSchedule { get; set; }

        public string Diagnostic { get; set; }
        
    }
}
