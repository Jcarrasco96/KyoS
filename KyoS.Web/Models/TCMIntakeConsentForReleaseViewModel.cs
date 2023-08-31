using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMIntakeConsentForReleaseViewModel : TCMIntakeConsentForReleaseEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdTCMClient { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Classification")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Classification.")]

        public int Idtype { get; set; }

        public IEnumerable<SelectListItem> ConsentList { get; set; }
    }
}
