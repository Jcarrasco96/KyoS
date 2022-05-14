using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class BioViewModel : BioEntity
    {
        [Display(Name = "Client")]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Client.")]

        public int IdClient { get; set; }

        [Display(Name = "Appetite")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Appetite.")]

        public int IdAppetite { get; set; }

        public IEnumerable<SelectListItem> Appetite_Status { get; set; }

        [Display(Name = "Hydration")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Hydration.")]

        public int IdHydratation { get; set; }

        public IEnumerable<SelectListItem> Hydratation_Status { get; set; }

        [Display(Name = "Recent Weight Change")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a Recent Weight Change.")]

        public int IdRecentWeight { get; set; }

        public IEnumerable<SelectListItem> RecentWeight_Status { get; set; }

        [Display(Name = "If Sexually Active")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]

        public int IdIfSexuallyActive { get; set; }

        public IEnumerable<SelectListItem> IfSexuallyActive_Status { get; set; }

        public string ReferralName { get; set; }

        public string LegalGuardianName { get; set; }

        public string LegalGuardianTelephone { get; set; }

        public string EmergencyContactName { get; set; }

        public string EmergencyContactTelephone { get; set; }

        public string RelationShipOfEmergencyContact { get; set; }
    }
}
