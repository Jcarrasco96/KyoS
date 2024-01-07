using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class MeetingNoteEntity
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string PlanNote { get; set; }
        public string Description { get; set; }
        public NoteStatus Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        
        public List<MeetingNotes_Facilitator> FacilitatorList { get; set; }
        public SupervisorEntity Supervisor { get; set; }

    }
}
