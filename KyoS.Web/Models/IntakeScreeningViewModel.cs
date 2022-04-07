using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class IntakeScreeningViewModel : IntakeScreeningEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdClient { get; set; }

        [Display(Name = "Client is")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        
        public int IdClientIs { get; set; }

        [Display(Name = "Behavior is")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]

        public int IdBehaviorIs { get; set; }

        [Display(Name = "Speech is")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]

        public int IdSpeechIs { get; set; }

        public IEnumerable<SelectListItem> Clients { get; set; }
        public IEnumerable<SelectListItem> ClientIs_Status { get; set; }
        public IEnumerable<SelectListItem> BehaviorIs_Status { get; set; }
        public IEnumerable<SelectListItem> SpeechIs_Status { get; set; }

    }
}
