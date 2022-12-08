using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Models
{
    public class NotPresentNotesClinicViewModel
    {
        public string DateIterval { get; set; }

        public int IdFacilitator { get; set; }
        public IEnumerable<SelectListItem> Facilitators { get; set; }

        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        public List<Workday_Client> WorkDaysClients { get; set; }
    }
}
