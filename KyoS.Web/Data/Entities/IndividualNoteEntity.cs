using KyoS.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class IndividualNoteEntity
    {
        public int Id { get; set; }

        public int Workday_Client_FK { get; set; }
        public Workday_Client Workday_Cient { get; set; }

        [Display(Name = "Subjective Data / Clinical Impression")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string SubjectiveData { get; set; }

        [Display(Name = "Objective Data / Behavioral Observations")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ObjectiveData { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Assessment { get; set; }

        [Display(Name = "Plan")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string PlanNote { get; set; }        

        public NoteStatus Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfApprove { get; set; }

        //Client mental status
        public bool Groomed { get; set; }
        public bool Unkempt { get; set; }
        public bool Disheveled { get; set; }
        public bool Meticulous { get; set; }
        public bool Overbuild { get; set; }
        public bool Other { get; set; }

        public bool Clear { get; set; }
        public bool Pressured { get; set; }
        public bool Slurred { get; set; }
        public bool Slow { get; set; }
        public bool Impaired { get; set; }
        public bool Poverty { get; set; }

        public bool Euthymic { get; set; }
        public bool Depressed { get; set; }
        public bool Anxious { get; set; }
        public bool Fearful { get; set; }
        public bool Irritable { get; set; }
        public bool Labile { get; set; }

        public bool WNL { get; set; }
        public bool Guarded { get; set; }
        public bool Withdrawn { get; set; }
        public bool Hostile { get; set; }
        public bool Restless { get; set; }
        public bool Impulsive { get; set; }

        public bool WNL_Cognition { get; set; }
        public bool Blocked { get; set; }
        public bool Obsessive { get; set; }
        public bool Paranoid { get; set; }
        public bool Scattered { get; set; }
        public bool Psychotic { get; set; }

        public bool Exceptional { get; set; }
        public bool Steady { get; set; }
        public bool Slow_Progress { get; set; }
        public bool Regressing { get; set; }
        public bool Stable { get; set; }
        public bool Maintain { get; set; }

        public bool CBT { get; set; }
        public bool Psychodynamic { get; set; }
        public bool BehaviorModification { get; set; }
        public bool Other_Intervention { get; set; }

        public SupervisorEntity Supervisor { get; set; }
        public ObjetiveEntity Objective { get; set; }

        public int? MTPId { get; set; }
    }
}
