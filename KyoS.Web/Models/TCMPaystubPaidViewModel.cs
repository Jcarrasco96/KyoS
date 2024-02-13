using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class TCMPaystubPaidViewModel : TCMPayStubEntity
    {
        public int IdTCMPaystub { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
       
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
    }
}
