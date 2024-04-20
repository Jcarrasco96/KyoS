﻿using KyoS.Web.Data.Entities;
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

        public int IdAgencyService { get; set; }
        public IEnumerable<SelectListItem> AgencyServices { get; set; }

        public int IdInsuranceType { get; set; }
        public IEnumerable<SelectListItem> InsuranceTypes { get; set; }

        public int IdInsurancePlanType { get; set; }
        public IEnumerable<SelectListItem> InsurancePlanTypes { get; set; }
    }
}
