using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class Workday_Activity_Facilitator4ViewModel
    {
        public int IdWorkday { get; set; }

        public string Date { get; set; }

        public string Day { get; set; }

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
        [Display(Name = "Topic #2")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a topic.")]
        public int IdTopic2 { get; set; }
        public IEnumerable<SelectListItem> Topics2 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Activity #2")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity2 { get; set; }
        public IEnumerable<SelectListItem> Activities2 { get; set; }

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
    }
}