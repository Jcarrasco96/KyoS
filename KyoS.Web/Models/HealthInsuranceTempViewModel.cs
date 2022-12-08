using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class HealthInsuranceTempViewModel : HealthInsuranceTempEntity
    {
        [Display(Name = "Health Insurance")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int IdhealthInsurance { get; set; }
        public IEnumerable<SelectListItem> HealthInsurance { get; set; }

        [Display(Name = "Client")]
        public string ClientName { get; set; }
    }
}
