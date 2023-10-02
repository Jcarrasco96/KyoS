using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMServicePlanViewModel : TCMServicePlanEntity
    {

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int ID_Status { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int ID_TcmClient { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int ID_Clinic { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string CaseNumber { get; set; }

        public IEnumerable<SelectListItem> TcmClients { get; set; }
        public IEnumerable<SelectListItem> Clinics { get; set; }
        public IEnumerable<SelectListItem> status { get; set; }

        public bool Domain1 { get; set; }
        public bool Domain2 { get; set; }
        public bool Domain3 { get; set; }
        public bool Domain4 { get; set; }
        public bool Domain5 { get; set; }
        public bool Domain6 { get; set; }
        public bool Domain7 { get; set; }
        public bool Domain8 { get; set; }
        public bool Domain9 { get; set; }
        public bool Domain10 { get; set; }
        public bool Domain11 { get; set; }
        public bool Domain12 { get; set; }

    }
}
