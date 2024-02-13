using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class MeetingNotes_Facilitator
    {
        public int Id { get; set; }
        public FacilitatorEntity Facilitator { get; set; }
        public MeetingNoteEntity MeetingNoteEntity { get; set; }
      
        public string Intervention { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateSign { get; set; }
        public bool Sign { get; set; }
    }
}
