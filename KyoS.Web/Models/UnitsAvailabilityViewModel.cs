using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class UnitsAvailabilityViewModel : Client_HealthInsurance
    {
        [Display(Name = "Health Insurance")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a insurance.")]
        public int IdHealthInsurance { get; set; }
        public IEnumerable<SelectListItem> HealthInsurances { get; set; }

        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a client.")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        public int UsedUnits { get; set; }

        public bool Expired { get; set; }        
    }
}
