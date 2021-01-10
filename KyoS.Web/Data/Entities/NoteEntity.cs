using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class NoteEntity
    {
        public int Id { get; set; }
        public Workday_Client Workday_Cient { get; set; }
        [Display(Name = "Plan")]
        public string PlanNote { get; set; }
        public NoteStatus Status { get; set; }
        public ICollection<Note_Activity> Notes_Activities { get; set; }
    }
}
