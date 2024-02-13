using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KyoS.Web.Models
{
    public class FacilitatorChanageViewModel
    {
        [Display(Name = "Clients")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients{ get; set; }

        public ClientEntity Client { get; set; }
    }
}
