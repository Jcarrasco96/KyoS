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
    public class TCMDomainViewModel : TCMDomainEntity
    {
        public string Needs_Identified { get; set; }
        public string Long_Term { get; set; }

        public DateTime Date_Identified { get; set; }

        public int Id_ServicePlan { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Id_Service { get; set; }
        public IEnumerable<SelectListItem> Services { get; set; }
    }
}
