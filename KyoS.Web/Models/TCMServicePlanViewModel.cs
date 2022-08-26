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
    }
}
