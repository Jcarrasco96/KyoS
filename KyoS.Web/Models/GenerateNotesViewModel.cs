using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class GenerateNotesViewModel
    {
        [Display(Name = "Group")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a group.")]
        public int IdGroup { get; set; }
        public IEnumerable<SelectListItem> Groups { get; set; }

        [Display(Name = "Day")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a day.")]
        public int DayId { get; set; }
        public IEnumerable<SelectListItem> Days { get; set; }

        [Display(Name = "Date")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}
