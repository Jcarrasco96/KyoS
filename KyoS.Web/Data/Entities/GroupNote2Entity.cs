using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class GroupNote2Entity
    {
        public int Id { get; set; }

        public int Workday_Client_FK { get; set; }
        public Workday_Client Workday_Cient { get; set; }

        public NoteStatus Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfApprove { get; set; }

        //Group Leader Intervention
        public bool GroupLeaderFacilitator { get; set; }
        public string GroupLeaderFacilitatorAbout { get; set; }
        public bool Facilitated { get; set; }
        public bool Involved { get; set; }
        public bool Kept { get; set; }
        public bool GroupLeaderProviderPsychoeducation { get; set; }
        public bool GroupLeaderProviderSupport { get; set; }
        public bool Assigned { get; set; }
        public string AssignedTopicOf { get; set; }

        //Progress Made Towards Goal
        public bool SignificantProgress { get; set; }
        public bool ModerateProgress { get; set; }
        public bool MinimalProgress { get; set; }
        public bool NoProgress { get; set; }
        public bool Regression { get; set; }
        public bool Descompensating { get; set; }
        public bool UnableToDetermine { get; set; }
       
        //Client Appearance
        public bool Oriented { get; set; }
        public bool NotToTime { get; set; }
        public bool NotToPlace { get; set; }
        public bool NotToPerson { get; set; }

        public bool Fair { get; set; }
        public bool InsightAdequate { get; set; }
        public bool Limited { get; set; }
        public bool Impaired { get; set; }
        public bool Faulty { get; set; }

        public bool Euthymic { get; set; }
        public bool Congruent { get; set; }
        public bool Euphoric { get; set; }
        public bool Optimistic { get; set; }
        public bool Hostile { get; set; }
        public bool Withdrawn { get; set; }

        public bool Negativistic { get; set; }
        public bool Depressed { get; set; }
        public bool Anxious { get; set; }
        public bool Irritable { get; set; }
        public bool Dramatic { get; set; }

        public bool Adequated { get; set; }
        public bool Inadequated { get; set; }
        public bool FairAttitude { get; set; }
        public bool Unmotivated { get; set; }
        public bool Motivated { get; set; }
        public bool Guarded { get; set; }

        public bool Normal { get; set; }
        public bool Short { get; set; }
        public bool MildlyImpaired { get; set; }
        public bool SevereryImpaired { get; set; }

        //Client Benefited by
        public bool Getting { get; set; }
        public bool Sharing { get; set; }
        public bool Expressing { get; set; }
        public bool LearningFrom { get; set; }
        public bool Developing { get; set; }
        public bool Received { get; set; }
        public bool Providing { get; set; }
        public bool LearningAbout { get; set; }
        public bool Other { get; set; }
        public string OtherExplain { get; set; }

        public SupervisorEntity Supervisor { get; set; }
        public ICollection<GroupNote2_Activity> GroupNotes2_Activities { get; set; }

        public int? MTPId { get; set; }
        public SchemaTypeGroup Schema { get; set; }
        public string Setting { get; set; }
    }
}
