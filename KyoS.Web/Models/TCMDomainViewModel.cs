using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMDomainViewModel : TCMDomainEntity
    {
        public string Needs_Identified { get; set; }
        public string Long_Term { get; set; }

        public int Id_ServicePlan { get; set; }


        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a service.")]
        public int Id_Service { get; set; }
        public IEnumerable<SelectListItem> Services { get; set; }

        public int Id_SubService { get; set; }
        public IEnumerable<SelectListItem> SubServices { get; set; }

        public string ChangeUpdate { get; set; }
    }
}
