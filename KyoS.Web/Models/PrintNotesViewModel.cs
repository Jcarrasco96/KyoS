using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class PrintNotesViewModel
    {
        [Display(Name = "Print's date")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = false)]
        public DateTime DateOfPrint { get; set; }
    }
}
