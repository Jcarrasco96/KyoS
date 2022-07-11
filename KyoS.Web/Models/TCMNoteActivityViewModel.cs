using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMNoteActivityViewModel : TCMNoteActivityEntity
    {
        public int IdTCMNote { get; set; }
        public int IdTCMClient { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Minutes")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a time range.")]
        public string Minutes { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Setting")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a setting.")]
        public int IdSetting { get; set; }
        public IEnumerable<SelectListItem> SettingList { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Domains")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a domain.")]
        public int IdTCMDomain { get; set; }
        public IEnumerable<SelectListItem> DomainList { get; set; }
    }
}
