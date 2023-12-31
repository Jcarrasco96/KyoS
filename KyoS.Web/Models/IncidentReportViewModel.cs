using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class IncidentReportViewModel : IncidentReportEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdClient { get; set; }
        public int IdFacilitator { get; set; }
        public int IdSupervisor { get; set; }
        public int IdDocumentAssisstant { get; set; }

        public IEnumerable<SelectListItem> Clients { get; set; }
    }
}
