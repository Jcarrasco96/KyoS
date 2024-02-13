using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMBillingViewModel
    {
        [Display(Name = "Client")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        [Display(Name = "TCM")]
        public int IdCaseManager { get; set; }
        public IEnumerable<SelectListItem> CaseManagers { get; set; }

        public string StartDate { get; set; }
    }
}
