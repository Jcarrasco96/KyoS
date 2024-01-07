using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class MeetingNotesViewModel : MeetingNoteEntity
    {
        public int IdSupervisor { get; set; }
    }
}
