using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class Workday_Activity_Facilitator3ViewModel
    {
        public int IdWorkday { get; set; }

        public string Date { get; set; }

        public string Day { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #1")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a theme.")]
        public int IdTopic1 { get; set; }
        public IEnumerable<SelectListItem> Topics1 { get; set; }

        [Required(ErrorMessage = "The field is mandatory.")]
        [Display(Name = "Activity #1")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity1 { get; set; }
        public IEnumerable<SelectListItem> Activities1 { get; set; }
        
        public bool copingSkills1 { get; set; }
        public bool stressManagement1 { get; set; }
        public bool healthyLiving1 { get; set; }
        public bool relaxationTraining1 { get; set; }
        public bool diseaseManagement1 { get; set; }
        public bool communityResources1 { get; set; }
        public bool activityDailyLiving1 { get; set; }
        public bool socialSkills1 { get; set; }
        public bool lifeSkills1 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #2")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a theme.")]
        public int IdTopic2 { get; set; }
        public IEnumerable<SelectListItem> Topics2 { get; set; }

        [Required(ErrorMessage = "The field is mandatory.")]
        [Display(Name = "Activity #2")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity2 { get; set; }
        public IEnumerable<SelectListItem> Activities2 { get; set; }

        public bool copingSkills2 { get; set; }
        public bool stressManagement2 { get; set; }
        public bool healthyLiving2 { get; set; }
        public bool relaxationTraining2 { get; set; }
        public bool diseaseManagement2 { get; set; }
        public bool communityResources2 { get; set; }
        public bool activityDailyLiving2 { get; set; }
        public bool socialSkills2 { get; set; }
        public bool lifeSkills2 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #3")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a theme.")]
        public int IdTopic3 { get; set; }
        public IEnumerable<SelectListItem> Topics3 { get; set; }

        [Required(ErrorMessage = "The field is mandatory.")]
        [Display(Name = "Activity #3")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity3 { get; set; }
        public IEnumerable<SelectListItem> Activities3 { get; set; }

        public bool copingSkills3 { get; set; }
        public bool stressManagement3 { get; set; }
        public bool healthyLiving3 { get; set; }
        public bool relaxationTraining3 { get; set; }
        public bool diseaseManagement3 { get; set; }
        public bool communityResources3 { get; set; }
        public bool activityDailyLiving3 { get; set; }
        public bool socialSkills3 { get; set; }
        public bool lifeSkills3 { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Topic #4")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a theme.")]
        public int IdTopic4 { get; set; }
        public IEnumerable<SelectListItem> Topics4 { get; set; }

        [Required(ErrorMessage = "The field is mandatory.")]
        [Display(Name = "Activity #4")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdActivity4 { get; set; }
        public IEnumerable<SelectListItem> Activities4 { get; set; }

        public bool copingSkills4 { get; set; }
        public bool stressManagement4 { get; set; }
        public bool healthyLiving4 { get; set; }
        public bool relaxationTraining4 { get; set; }
        public bool diseaseManagement4 { get; set; }
        public bool communityResources4 { get; set; }
        public bool activityDailyLiving4 { get; set; }
        public bool socialSkills4 { get; set; }
        public bool lifeSkills4 { get; set; }
    }
}
