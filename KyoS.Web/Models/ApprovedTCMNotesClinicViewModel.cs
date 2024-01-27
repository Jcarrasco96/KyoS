using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ApprovedTCMNotesClinicViewModel
    {
        public string DateIterval { get; set; }

        public int IdCaseManager { get; set; }
        public IEnumerable<SelectListItem> CaseManagers { get; set; }

        public int IdTCMClient { get; set; }
        public IEnumerable<SelectListItem> TCMClients { get; set; }

        public List<TCMNoteEntity> TCMNotes { get; set; }
    }
}
