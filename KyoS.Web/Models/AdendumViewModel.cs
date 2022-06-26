using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class AdendumViewModel : AdendumEntity
    {
        [Display(Name = "MTP")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a MTP.")]
        public int IdMTP { get; set; }
        public IEnumerable<SelectListItem> MTPs { get; set; }

        public int IdSupervisor { get; set; }
        public int IdFacilitator { get; set; }

        public int Origin { get; set; }
    }
}
