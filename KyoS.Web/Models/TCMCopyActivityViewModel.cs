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
    public class TCMCopyActivityViewModel 
    {
        public int IdServiceTo { get; set; }
        public TCMServiceEntity TCMService { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "ServiceFrom")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Service.")]

        public int IdServiceFrom { get; set; }

        public IEnumerable<SelectListItem> Services { get; set; }

    }
}
