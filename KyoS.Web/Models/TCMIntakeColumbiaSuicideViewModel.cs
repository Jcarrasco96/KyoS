using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace KyoS.Web.Models
{
    public class TCMIntakeColumbiaSuicideViewModel : TCMIntakeColumbiaSuicideEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdTCMClient { get; set; }

        public IEnumerable<SelectListItem> TcmClients { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouWishedPastMonth_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouWishedLifeTime_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouActuallyPastMonth_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouActuallyLifeTime_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouBeenPastMonth_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouBeenLifeTime_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouHadPastMonth_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouHadLifeTime_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouStartedPastMonth_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouStartedLifeTime_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouEver_Value { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Risk")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Risk.")]
        public int IdHaveYouEverIfYes_Value { get; set; }

        public IEnumerable<SelectListItem> RiskList { get; set; }
    }
}
