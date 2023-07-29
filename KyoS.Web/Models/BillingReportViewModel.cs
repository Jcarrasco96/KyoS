using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using System.Data;
using KyoS.Common.Enums;

namespace KyoS.Web.Models
{
    public class BillingReportViewModel
    {
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string DateIterval { get; set; }

        public int IdFacilitator { get; set; }
        public IEnumerable<SelectListItem> Facilitators { get; set; }

        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        public List<WeekEntity> Weeks { get; set; }

        public int IdService { get; set; }

        public IEnumerable<SelectListItem> Services { get; set; }

    }
}