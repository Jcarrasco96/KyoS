using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class IncidentViewModel : IncidentEntity
    {
        public string IdUserCreatedBy { get; set; }
        
        [Display(Name = "Status")]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a status.")]
        public int IdStatus { get; set; }

        public IEnumerable<SelectListItem> StatusList { get; set; }

        [Display(Name = "Client")]
        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        [Display(Name = "Assigned to")]
        public string IdUserAssigned { get; set; }
        public IEnumerable<SelectListItem> Users { get; set; }
    }
}
