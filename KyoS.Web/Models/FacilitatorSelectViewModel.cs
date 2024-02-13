using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KyoS.Web.Models
{
    public class FacilitatorSelectViewModel
    {
        [Display(Name = "Facilitators")]
        public int IdFacilitator { get; set; }
        public IEnumerable<SelectListItem> Facilitators{ get; set; }

        public Workday_Client Workday_Client { get; set; }

        public int IdWorkdayClient { get; set; }
    }
}
