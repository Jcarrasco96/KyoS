using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class BriefViewModel : BriefEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdClient { get; set; }

        public string ReferralName { get; set; }

        public string LegalGuardianName { get; set; }

        public string LegalGuardianTelephone { get; set; }

        public string EmergencyContactName { get; set; }

        public string EmergencyContactTelephone { get; set; }

        public string RelationShipOfEmergencyContact { get; set; }
    }
}
