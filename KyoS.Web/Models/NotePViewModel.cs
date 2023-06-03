using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class NotePViewModel : NotePEntity
    {
        public int IdActivity1 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal1 { get; set; }
        public IEnumerable<SelectListItem> Goals1 { get; set; }

        [Display(Name = "Objective")]
        public int IdObjetive1 { get; set; }
        public IEnumerable<SelectListItem> Objetives1 { get; set; }

        public string Intervention1 { get; set; }

        //Assistance of client for activity # 1
        public bool Present1 { get; set; }

        //Client's response 1
        public bool Cooperative1 { get; set; }
        public bool Assertive1 { get; set; }
        public bool Passive1 { get; set; }
        public bool Variable1 { get; set; }
        public bool Uninterested1 { get; set; }
        public bool Engaged1 { get; set; }
        public bool Distractible1 { get; set; }
        public bool Confused1 { get; set; }
        public bool Aggresive1 { get; set; }
        public bool Resistant1 { get; set; }
        public bool Other1 { get; set; }

        //Skill sets addressed 1 (readonly)
        public bool copingSkills1 { get; set; }
        public bool stressManagement1 { get; set; }
        public bool healthyLiving1 { get; set; }
        public bool relaxationTraining1 { get; set; }
        public bool diseaseManagement1 { get; set; }
        public bool communityResources1 { get; set; }
        public bool activityDailyLiving1 { get; set; }
        public bool socialSkills1 { get; set; }
        public bool lifeSkills1 { get; set; }

        public int IdActivity2 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal2 { get; set; }
        public IEnumerable<SelectListItem> Goals2 { get; set; }

        [Display(Name = "Objective")]
        public int IdObjetive2 { get; set; }
        public IEnumerable<SelectListItem> Objetives2 { get; set; }

        public string Intervention2 { get; set; }

        //Assistance of client for activity # 2
        public bool Present2 { get; set; }

        //Client's response 2
        public bool Cooperative2 { get; set; }
        public bool Assertive2 { get; set; }
        public bool Passive2 { get; set; }
        public bool Variable2 { get; set; }
        public bool Uninterested2 { get; set; }
        public bool Engaged2 { get; set; }
        public bool Distractible2 { get; set; }
        public bool Confused2 { get; set; }
        public bool Aggresive2 { get; set; }
        public bool Resistant2 { get; set; }
        public bool Other2 { get; set; }

        //Skill sets addressed 2 (readonly)
        public bool copingSkills2 { get; set; }
        public bool stressManagement2 { get; set; }
        public bool healthyLiving2 { get; set; }
        public bool relaxationTraining2 { get; set; }
        public bool diseaseManagement2 { get; set; }
        public bool communityResources2 { get; set; }
        public bool activityDailyLiving2 { get; set; }
        public bool socialSkills2 { get; set; }
        public bool lifeSkills2 { get; set; }

        public int IdActivity3 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal3 { get; set; }
        public IEnumerable<SelectListItem> Goals3 { get; set; }

        [Display(Name = "Objective")]
        public int IdObjetive3 { get; set; }
        public IEnumerable<SelectListItem> Objetives3 { get; set; }
        public string Intervention3 { get; set; }

        //Assistance of client for activity # 3
        public bool Present3 { get; set; }

        //Client's response 3
        public bool Cooperative3 { get; set; }
        public bool Assertive3 { get; set; }
        public bool Passive3 { get; set; }
        public bool Variable3 { get; set; }
        public bool Uninterested3 { get; set; }
        public bool Engaged3 { get; set; }
        public bool Distractible3 { get; set; }
        public bool Confused3 { get; set; }
        public bool Aggresive3 { get; set; }
        public bool Resistant3 { get; set; }
        public bool Other3 { get; set; }

        //Skill sets addressed 3 (readonly)
        public bool copingSkills3 { get; set; }
        public bool stressManagement3 { get; set; }
        public bool healthyLiving3 { get; set; }
        public bool relaxationTraining3 { get; set; }
        public bool diseaseManagement3 { get; set; }
        public bool communityResources3 { get; set; }
        public bool activityDailyLiving3 { get; set; }
        public bool socialSkills3 { get; set; }
        public bool lifeSkills3 { get; set; }

        public int IdActivity4 { get; set; }

        [Display(Name = "Goal")]
        public int IdGoal4 { get; set; }
        public IEnumerable<SelectListItem> Goals4 { get; set; }

        [Display(Name = "Objective")]
        public int IdObjetive4 { get; set; }
        public IEnumerable<SelectListItem> Objetives4 { get; set; }
        public string Intervention4 { get; set; }

        //Assistance of client for activity # 4
        public bool Present4 { get; set; }

        //Client's response 4
        public bool Cooperative4 { get; set; }
        public bool Assertive4 { get; set; }
        public bool Passive4 { get; set; }
        public bool Variable4 { get; set; }
        public bool Uninterested4 { get; set; }
        public bool Engaged4 { get; set; }
        public bool Distractible4 { get; set; }
        public bool Confused4 { get; set; }
        public bool Aggresive4 { get; set; }
        public bool Resistant4 { get; set; }
        public bool Other4 { get; set; }

        //Skill sets addressed 4 (readonly)
        public bool copingSkills4 { get; set; }
        public bool stressManagement4 { get; set; }
        public bool healthyLiving4 { get; set; }
        public bool relaxationTraining4 { get; set; }
        public bool diseaseManagement4 { get; set; }
        public bool communityResources4 { get; set; }
        public bool activityDailyLiving4 { get; set; }
        public bool socialSkills4 { get; set; }
        public bool lifeSkills4 { get; set; }

        //este campo lo uso para saber de que pagina se viene
        public int Origin { get; set; }

        //read only fields
        public string Theme1 { get; set; }
        public string FacilitatorIntervention1 { get; set; }
        public string Goal1 { get; set; }
        public string Objetive1 { get; set; }
        public string Theme2 { get; set; }
        public string FacilitatorIntervention2 { get; set; }
        public string Goal2 { get; set; }
        public string Objetive2 { get; set; }
        public string Theme3 { get; set; }
        public string FacilitatorIntervention3 { get; set; }
        public string Goal3 { get; set; }
        public string Objetive3 { get; set; }
        public string Theme4 { get; set; }
        public string FacilitatorIntervention4 { get; set; }
        public string Goal4 { get; set; }
        public string Objetive4 { get; set; }

        public string CodeBill { get; set; }

    }
}
