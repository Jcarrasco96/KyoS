using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class CalendarCMH_DocAssistant
    {
        [Display(Name = "Client")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        [Display(Name = "Facilitator")]
        public int IdDocumentAssistant { get; set; }
        public IEnumerable<SelectListItem> DocumentAssistants { get; set; }
    }
}
