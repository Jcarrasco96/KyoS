using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class BillDmsPaidViewModel : BillDmsPaidEntity
    {
        public int IdBillDms { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Origi.")]
        public int IdOrigi { get; set; }
        public IEnumerable<SelectListItem> OrigiList { get; set; }
    }
}
