using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMBillingReportViewModel
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string DateIterval { get; set; }

        public int IdCaseManager { get; set; }
        public IEnumerable<SelectListItem> CaseManagers { get; set; }

        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        public List<TCMNoteEntity> TCMNotes { get; set; }
    }
}
