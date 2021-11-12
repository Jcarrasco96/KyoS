using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class PaymentReceivedViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Payment Date")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }
    }
}
