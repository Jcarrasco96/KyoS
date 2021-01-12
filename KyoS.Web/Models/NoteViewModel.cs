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
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #1")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a topic.")]
        public int IdTopic1 { get; set; }
        public IEnumerable<SelectListItem> Topics1 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Activity #1")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity1 { get; set; }
        public IEnumerable<SelectListItem> Activities1 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Client Answer #1")]
        public string AnswerClient1 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Facilitator Answer #1")]
        public string AnswerFacilitator1 { get; set; }

        
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #2")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a topic.")]
        public int IdTopic2 { get; set; }
        public IEnumerable<SelectListItem> Topics2 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Activity #2")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity2 { get; set; }
        public IEnumerable<SelectListItem> Activities2 { get; set; }

        [Display(Name = "Client Answer #2")]
        public string AnswerClient2 { get; set; }

        [Display(Name = "Facilitator Answer #2")]
        public string AnswerFacilitator2 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #3")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a topic.")]
        public int IdTopic3 { get; set; }
        public IEnumerable<SelectListItem> Topics3 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Activity #3")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity3 { get; set; }
        public IEnumerable<SelectListItem> Activities3 { get; set; }

        [Display(Name = "Client Answer #3")]
        public string AnswerClient3 { get; set; }

        [Display(Name = "Facilitator Answer #3")]
        public string AnswerFacilitator3 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #4")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a topic.")]
        public int IdTopic4 { get; set; }
        public IEnumerable<SelectListItem> Topics4 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Activity #4")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity4 { get; set; }
        public IEnumerable<SelectListItem> Activities4 { get; set; }

        [Display(Name = "Client Answer #4")]
        public string AnswerClient4 { get; set; }

        [Display(Name = "Facilitator Answer #4")]
        public string AnswerFacilitator4 { get; set; }

        //estos campos solo se usan para el modo solo lectura
        public string Topic1 { get; set; }
        public string Activity1 { get; set; }
        public string Topic2 { get; set; }
        public string Activity2 { get; set; }
        public string Topic3 { get; set; }
        public string Activity3 { get; set; }
        public string Topic4 { get; set; }
        public string Activity4 { get; set; }
    }
}
