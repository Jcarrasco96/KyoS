using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMClientViewModel : TCMClientEntity
    {
        [Display(Name = "CaseMannager")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a CaseMannager.")]
        public int IdCaseMannager { get; set; }
        public IEnumerable<SelectListItem> CaseMannagers { get; set; }

        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
    }
}
