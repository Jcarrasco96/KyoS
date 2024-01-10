using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class MeetingNotesFacilitatorModel : MeetingNotes_Facilitator
    {
        public int IdSupervisorNote { get; set; }
    }
}
