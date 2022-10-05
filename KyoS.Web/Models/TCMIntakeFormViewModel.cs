using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMIntakeFormViewModel : TCMIntakeFormEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdTCMClient { get; set; }

        public IEnumerable<SelectListItem> TcmClients { get; set; }

        [Display(Name = "Relationship with emergency contact")]
        public int IdRelationshipEC { get; set; }
        public IEnumerable<SelectListItem> RelationshipsEC { get; set; }

        [Required(ErrorMessage = "The field is mandatory.")]
        public string EmergencyContactName { get; set; }

        [Required(ErrorMessage = "The field is mandatory.")]
        public string EmergencyContacTelephone { get; set; }

        public string LegalGuardianName { get; set; }
        public string LegalGuardianTelephone { get; set; }
        public string LegalGuardianAddress { get; set; }
        public string LegalGuardianCity { get; set; }
        public string LegalGuardianState { get; set; }
        public string LegalGuardianZipCode { get; set; }

        [Display(Name = "Relationship with legal guardian")]
        public int IdRelationshipLG { get; set; }
        public IEnumerable<SelectListItem> RelationshipsLG { get; set; }

    }
    
}
