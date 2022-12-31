using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class Client_DiagnosticViewModel : Client_Diagnostic
    {
        [Display(Name = "Diagnostic")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int IdDiagnostic { get; set; }
        public IEnumerable<SelectListItem> Diagnostics { get; set; }
        public int IdClient { get; set; }
    }
}
