using KyoS.Web.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMFarsFormViewModel : TCMFarsFormEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdTCMClient { get; set; }
        public int IdTCMSupervisor { get; set; }
        public int Origin { get; set; }
    }
}
