using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class NotePEntity
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public int Workday_Client_FK { get; set; }
        public Workday_Client Workday_Cient { get; set; }

        [Display(Name = "Client benefited from the above groups by:")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string PlanNote { get; set; }
        public NoteStatus Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfApprove { get; set; }

        //Client mental status
        public bool Attentive { get; set; }
        public bool Depressed { get; set; }
        public bool Inattentive { get; set; }
        public bool Angry { get; set; }
        public bool Sad { get; set; }
        public bool FlatAffect { get; set; }
        public bool Anxious { get; set; }
        public bool PositiveEffect { get; set; }
        public bool Oriented3x { get; set; }
        public bool Oriented2x { get; set; }
        public bool Oriented1x { get; set; }
        public bool Impulsive { get; set; }
        public bool Labile { get; set; }
        public bool Withdrawn { get; set; }
        public bool RelatesWell { get; set; }        
        public bool DecreasedEyeContact { get; set; }
        public bool AppropiateEyeContact { get; set; }

        //Progress
        public bool Minimal { get; set; }
        public bool Slow { get; set; }
        public bool Steady { get; set; }
        public bool GoodExcelent { get; set; }
        public bool IncreasedDifficultiesNoted { get; set; }
        public bool Complicated { get; set; }
        public bool DevelopingInsight { get; set; }
        public bool LittleInsight { get; set; }
        public bool Aware { get; set; }
        public bool AbleToGenerateAlternatives { get; set; }
        public bool Initiates { get; set; }
        public bool ProblemSolved { get; set; }
        public bool DemostratesEmpathy { get; set; }
        public bool UsesSessions { get; set; }
        public bool Variable { get; set; }

        public string Setting { get; set; }

        public SupervisorEntity Supervisor { get; set; }
        public ICollection<NoteP_Activity> NotesP_Activities { get; set; }

        public int? MTPId { get; set; }

        public SchemaType Schema { get; set; }

        public int RealUnits { get; set; }
    }
}
