using KyoS.Web.Data.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class FarsFormViewModel : FarsFormEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdClient { get; set; }
        public int IdSupervisor { get; set; }
        public int Origin { get; set; }
    }
}
