using KyoS.Web.Data.Entities;
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
    public class BillDmsViewModel : BillDmsEntity
    {
        public int AmountTCMNotes { get; set; }
        public int AmountCMHNotes { get; set; }
        public int Units { get; set; }
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateBillClose { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }
    }
}
