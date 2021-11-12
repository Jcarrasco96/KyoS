using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class BillViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Billed Date")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [DataType(DataType.Date)]
        public DateTime BilledDate { get; set; }
    }
}
