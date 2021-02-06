using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class NotesSummary
    {
        public string FacilitatorName { get; set; }
        public int NotStarted { get; set; }
        public int Editing { get; set; }
        public int Pending { get; set; }
        public int Review { get; set; }
    }
}
