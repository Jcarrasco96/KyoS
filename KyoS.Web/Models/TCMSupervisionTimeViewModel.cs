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
    public class TCMSupervisionTimeViewModel : TCMSupervisionTimeEntity
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "CaseManager")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a casemanager.")]

        public int IdCaseManager { get; set; }
        public IEnumerable<SelectListItem> CaseManagers { get; set; }

        public int IdTCMSupervisor { get; set; }

    }
}
