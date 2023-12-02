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
        [Display(Name = "Setting")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a setting.")]
        public int IdSetting { get; set; }
        public IEnumerable<SelectListItem> SettingList { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Domains")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a domain.")]
        public int IdTCMDomain { get; set; }
        public IEnumerable<SelectListItem> DomainList { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Activities")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a activity.")]
        public int IdTCMActivity { get; set; }
        public IEnumerable<SelectListItem> ActivityList { get; set; }

        public string DescriptionTemp { get; set; }

        public DateTime DateOfServiceNote { get; set; }

        public int Units { get; set; }

        public string NeedIdentified { get; set; }

        public string TimeEnd { get; set; }

        public string SubService { get; set; }

    }
}
