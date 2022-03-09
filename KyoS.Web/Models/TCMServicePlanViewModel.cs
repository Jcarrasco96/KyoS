using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime Date_ServicePlan { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime Date_Intake { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime Date_Assessment { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime Date_Certification { get; set; }

        public string strengths { get; set; }

        public string weakness { get; set; }

        public string dischargerCriteria { get; set; }

        public IEnumerable<SelectListItem> TcmClients { get; set; }
        public IEnumerable<SelectListItem> Clinics { get; set; }
        public IEnumerable<SelectListItem> status { get; set; }
    }
}
